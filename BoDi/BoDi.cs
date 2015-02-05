/**************************************************************************************
 * 
 * BoDi: A very simple IoC container, easily embeddable also as a source code. 
 * 
 * BoDi was created to support SpecFlow (http://www.specflow.org) by Gaspar Nagy (http://gasparnagy.com/)
 * 
 * Project source & unit tests: http://github.com/gasparnagy/BoDi
 * License: Apache License 2.0
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 * 
 * Change history
 * 
 * v1.2
 *   - should not allow resolving value types (structs)
 *   - should list registrations in container ToString()
 *   - should not dispose registered instances by default, disposal can be requested by the 'dispose: true' parameter
 *   - smaller code refactoring
 *   - improve resolution path handling
 * 
 * v1.1 - released with SpecFlow v1.9.0
 * 
 */
using System;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace BoDi
{
#if !BODI_LIMITEDRUNTIME
    [Serializable]
#endif
    public class ObjectContainerException : Exception
    {
        public ObjectContainerException(string message, IEnumerable<Type> resolutionPath) : base(GetMessage(message, resolutionPath))
        {
        }

#if !BODI_LIMITEDRUNTIME
        protected ObjectContainerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif

        static private string GetMessage(string message, IEnumerable<Type> resolutionPath)
        {
            if (resolutionPath == null)
                return message;

            var resolutionPathArray = resolutionPath.ToArray();
            if (resolutionPathArray.Length == 0)
                return message;

            return string.Format("{0} (resolution path: {1})", message, string.Join("->", resolutionPathArray.Select(t => t.FullName).ToArray()));
        }
    }

    public interface IObjectContainer: IDisposable
    {
        /// <summary>
        /// Registers a type as the desired implementation type of an interface.
        /// </summary>
        /// <typeparam name="TType">Implementation type</typeparam>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <param name="name">A name to register named instance, otherwise null.</param>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref name="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <typeparamref name="TInterface"/>.</para>
        /// </remarks>
        void RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface;

        /// <summary>
        /// Registers an instance 
        /// </summary>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <param name="instance">The instance implements the interface.</param>
        /// <param name="name">A name to register named instance, otherwise null.</param>
        /// <param name="dispose">Whether the instance should be disposed on container dispose, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is null.</exception>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref name="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <typeparamref name="TInterface"/>.</para>
        ///     <para>The instance will be registered in the object pool, so if a <see cref="Resolve{T}()"/> (for another interface) would require an instance of the dynamic type of the <paramref name="instance"/>, the <paramref name="instance"/> will be returned.</para>
        /// </remarks>
        void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class;

        /// <summary>
        /// Registers an instance 
        /// </summary>
        /// <param name="instance">The instance implements the interface.</param>
        /// <param name="interfaceType">Interface will be resolved</param>
        /// <param name="name">A name to register named instance, otherwise null.</param>
        /// <param name="dispose">Whether the instance should be disposed on container dispose, otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is null.</exception>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <paramref name="interfaceType"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <paramref name="interfaceType"/>.</para>
        ///     <para>The instance will be registered in the object pool, so if a <see cref="Resolve{T}()"/> (for another interface) would require an instance of the dynamic type of the <paramref name="instance"/>, the <paramref name="instance"/> will be returned.</para>
        /// </remarks>
        void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false);

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref name="T"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        T Resolve<T>();

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <param name="name">A name to resolve named instance, otherwise null.</param>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref name="T"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        T Resolve<T>(string name);

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <param name="typeToResolve">The interface or type.</param>
        /// <param name="name">A name to resolve named instance, otherwise null.</param>
        /// <returns>An object implementing <paramref name="typeToResolve"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        object Resolve(Type typeToResolve, string name = null);
    }

    public interface IContainedInstance
    {
        IObjectContainer Container { get; }
    }

    public class ObjectContainer : IObjectContainer
    {
        private const string REGISTERED_NAME_PARAMETER_NAME = "registeredName";

        /// <summary>
        /// A very simple immutable linked list of <see cref="Type"/>.
        /// </summary>
        private class ResolutionList : IEnumerable<Type>
        {
            private readonly Type type;
            private readonly ResolutionList nextNode;
            private bool IsLast { get { return type == null; } }

            public ResolutionList()
            {
                Debug.Assert(IsLast);
            }

            private ResolutionList(Type type, ResolutionList nextNode)
            {
                if (type == null) throw new ArgumentNullException("type");
                if (nextNode == null) throw new ArgumentNullException("nextNode");

                this.type = type;
                this.nextNode = nextNode;
            }

            public ResolutionList AddToEnd(Type resolvedType)
            {
                if (resolvedType == null) throw new ArgumentNullException("resolvedType");
                return new ResolutionList(resolvedType, this);
            }

            public bool Contains(Type resolvedType)
            {
                if (resolvedType == null) throw new ArgumentNullException("resolvedType");
                return GetReverseEnumerable().Contains(resolvedType);
            }

            private IEnumerable<Type> GetReverseEnumerable()
            {
                var node = this;
                while (!node.IsLast)
                {
                    yield return node.type;
                    node = node.nextNode;
                }
            }

            public IEnumerator<Type> GetEnumerator()
            {
                return GetReverseEnumerable().Reverse().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private struct RegistrationKey
        {
            public readonly Type Type;
            public readonly string Name;

            public RegistrationKey(Type type, string name)
            {
                if (type == null) throw new ArgumentNullException("type");

                Type = type;
                Name = name;
            }

            public override string ToString()
            {
                Debug.Assert(Type.FullName != null);
                if (Name == null)
                    return Type.FullName;

                return string.Format("{0}('{1}')", Type.FullName, Name);
            }

            bool Equals(RegistrationKey other)
            {
                return other.Type == Type && String.Equals(other.Name, Name, StringComparison.CurrentCultureIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(RegistrationKey)) return false;
                return Equals((RegistrationKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Type.GetHashCode();
                }
            }
        }
        private interface IRegistration
        {
            object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);
        }

        private class TypeRegistration : IRegistration
        {
            private readonly Type implementationType;

            public TypeRegistration(Type implementationType)
            {
                this.implementationType = implementationType;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var pooledObjectKey = new RegistrationKey(implementationType, keyToResolve.Name);
                object obj = container.GetPooledObject(pooledObjectKey);

                if (obj == null)
                {
                    if (implementationType.IsInterface)
                        throw new ObjectContainerException("Interface cannot be resolved: " + keyToResolve, resolutionPath);

                    obj = container.CreateObject(implementationType, resolutionPath, keyToResolve);
                    container.objectPool.Add(pooledObjectKey, obj);
                }

                return obj;
            }

            public override string ToString()
            {
                return "Type: " + implementationType.FullName;
            }
        }

        private class InstanceRegistration : IRegistration
        {
            private readonly object instance;

            public InstanceRegistration(object instance)
            {
                this.instance = instance;
            }

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                return instance;
            }

            public override string ToString()
            {
                string instanceText;
                try
                {
                    instanceText = instance.ToString();
                }
                catch (Exception ex)
                {
                    instanceText = ex.Message;
                }

                return "Instance: " + instanceText;
            }
        }

        private class NonDisposableWrapper
        {
            public object Object { get; private set; }

            public NonDisposableWrapper(object obj)
            {
                Object = obj;
            }
        }

        private class NamedInstanceDictionaryRegistration : IRegistration
        {
            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToResolve = keyToResolve.Type;
                Debug.Assert(typeToResolve.IsGenericType && typeToResolve.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                var genericArguments = typeToResolve.GetGenericArguments();
                var keyType = genericArguments[0];
                var targetType = genericArguments[1];
                var result = (IDictionary)Activator.CreateInstance(typeof (Dictionary<,>).MakeGenericType(genericArguments));

                foreach (var namedRegistration in container.registrations.Where(r => r.Key.Name != null && r.Key.Type == targetType).Select(r => r.Key))
                {
                    var convertedKey = ChangeType(namedRegistration.Name, keyType);
                    Debug.Assert(convertedKey != null);
                    result.Add(convertedKey, container.Resolve(namedRegistration.Type, namedRegistration.Name));
                }

                return result;
            }

            private object ChangeType(string name, Type keyType)
            {
                if (keyType.IsEnum)
                    return Enum.Parse(keyType, name, true);

                Debug.Assert(keyType == typeof(string));
                return name;
            }
        }

        private bool isDisposed = false;
        private readonly ObjectContainer baseContainer;
        private readonly Dictionary<RegistrationKey, IRegistration> registrations = new Dictionary<RegistrationKey, IRegistration>();
        private readonly Dictionary<RegistrationKey, object> resolvedObjects = new Dictionary<RegistrationKey, object>();
        private readonly Dictionary<RegistrationKey, object> objectPool = new Dictionary<RegistrationKey, object>();

        public ObjectContainer(IObjectContainer baseContainer = null) 
        {
            if (baseContainer != null && !(baseContainer is ObjectContainer))
                throw new ArgumentException("Base container must be an ObjectContainer", "baseContainer");

            this.baseContainer = (ObjectContainer)baseContainer;
            RegisterInstanceAs<IObjectContainer>(this);
        }

        #region Registration

        public void RegisterTypeAs<TInterface>(Type implementationType, string name = null) where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            RegisterTypeAs(implementationType, interfaceType, name);
        }

        public void RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TType);
            RegisterTypeAs(implementationType, interfaceType, name);
        }

        private RegistrationKey CreateNamedInstanceDictionaryKey(Type targetType)
        {
            return new RegistrationKey(typeof(IDictionary<,>).MakeGenericType(typeof(string), targetType), null);
        }

        private void AddRegistration(RegistrationKey key, IRegistration registration)
        {
            registrations[key] = registration;

            if (key.Name != null)
            {
                var dictKey = CreateNamedInstanceDictionaryKey(key.Type);
                if (!registrations.ContainsKey(dictKey))
                {
                    registrations[dictKey] = new NamedInstanceDictionaryRegistration();
                }
            }
        }

        private void RegisterTypeAs(Type implementationType, Type interfaceType, string name)
        {
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);

            AddRegistration(registrationKey, new TypeRegistration(implementationType));
        }

        public void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            ClearRegistrations(registrationKey);
            AddRegistration(registrationKey, new InstanceRegistration(instance));
            objectPool[new RegistrationKey(instance.GetType(), name)] = GetPoolableInstance(instance, dispose);
        }

        private static object GetPoolableInstance(object instance, bool dispose)
        {
            return (instance is IDisposable) && !dispose ? new NonDisposableWrapper(instance) : instance;
        }

        public void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class
        {
            RegisterInstanceAs(instance, typeof(TInterface), name, dispose);
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertNotResolved(RegistrationKey interfaceType)
        {
            if (resolvedObjects.ContainsKey(interfaceType))
                throw new ObjectContainerException("An object have been resolved for this interface already.", null);
        }

        private void ClearRegistrations(RegistrationKey registrationKey)
        {
            registrations.Remove(registrationKey);
        }

#if !BODI_LIMITEDRUNTIME
        public void RegisterFromConfiguration()
        {
            var section = (BoDiConfigurationSection)ConfigurationManager.GetSection("boDi");
            if (section == null)
                return;

            RegisterFromConfiguration(section.Registrations);
        }

        public void RegisterFromConfiguration(ContainerRegistrationCollection containerRegistrationCollection)
        {
            if (containerRegistrationCollection == null)
                return;

            foreach (ContainerRegistrationConfigElement registrationConfigElement in containerRegistrationCollection)
            {
                RegisterFromConfiguration(registrationConfigElement);
            }
        }

        private void RegisterFromConfiguration(ContainerRegistrationConfigElement registrationConfigElement)
        {
            Type interfaceType = Type.GetType(registrationConfigElement.Interface, true);
            Type implementationType = Type.GetType(registrationConfigElement.Implementation, true);

            RegisterTypeAs(implementationType, interfaceType, string.IsNullOrEmpty(registrationConfigElement.Name) ? null : registrationConfigElement.Name);
        }
#endif

        #endregion

        #region Resolve

        public T Resolve<T>()
        {
            return Resolve<T>(null);
        }

        public T Resolve<T>(string name)
        {
            Type typeToResolve = typeof(T);

            object resolvedObject = Resolve(typeToResolve, name);

            return (T)resolvedObject;
        }

        public object Resolve(Type typeToResolve, string name = null)
        {
            return Resolve(typeToResolve, new ResolutionList(), name);
        }

        private object Resolve(Type typeToResolve, ResolutionList resolutionPath, string name)
        {
            AssertNotDisposed();

            var keyToResolve = new RegistrationKey(typeToResolve, name);
            object resolvedObject;
            if (!resolvedObjects.TryGetValue(keyToResolve, out resolvedObject))
            {
                resolvedObject = ResolveObject(keyToResolve, resolutionPath);
                resolvedObjects.Add(keyToResolve, resolvedObject);
            }
            Debug.Assert(typeToResolve.IsInstanceOfType(resolvedObject));
            return resolvedObject;
        }

        private IRegistration GetRegistrationResult(RegistrationKey keyToResolve)
        {
            IRegistration registration;
            if (registrations.TryGetValue(keyToResolve, out registration))
            {
                return registration;
            }

            if (baseContainer != null)
                return baseContainer.GetRegistrationResult(keyToResolve);

            if (IsSpecialNamedInstanceDictionaryKey(keyToResolve))
            {
                var targetType = keyToResolve.Type.GetGenericArguments()[1];
                return GetRegistrationResult(CreateNamedInstanceDictionaryKey(targetType));
            }

            // if there was no named registration, we still return an empty dictionary
            if (IsDefaultNamedInstanceDictionaryKey(keyToResolve))
            {
                return new NamedInstanceDictionaryRegistration();
            }

            return null;
        }

        private bool IsDefaultNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) && 
                   keyToResolve.Type.GetGenericArguments()[0] == typeof(string);
        }

        private bool IsSpecialNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) && 
                   keyToResolve.Type.GetGenericArguments()[0].IsEnum;
        }

        private bool IsNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name == null && keyToResolve.Type.IsGenericType && keyToResolve.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }

        private object GetPooledObject(RegistrationKey pooledObjectKey)
        {
            object obj;
            if (GetObjectFromPool(pooledObjectKey, out obj))
                return obj;

            if (baseContainer != null)
                return baseContainer.GetPooledObject(pooledObjectKey);

            return null;
        }

        private bool GetObjectFromPool(RegistrationKey pooledObjectKey, out object obj)
        {
            if (!objectPool.TryGetValue(pooledObjectKey, out obj))
                return false;

            var nonDisposableWrapper = obj as NonDisposableWrapper;
            if (nonDisposableWrapper != null)
                obj = nonDisposableWrapper.Object;

            return true;
        }

        private object ResolveObject(RegistrationKey keyToResolve, ResolutionList resolutionPath)
        {
            if (keyToResolve.Type.IsPrimitive || keyToResolve.Type == typeof(string) || keyToResolve.Type.IsValueType)
                throw new ObjectContainerException("Primitive types or structs cannot be resolved: " + keyToResolve.Type.FullName, resolutionPath);

            var registrationResult = GetRegistrationResult(keyToResolve) ?? new TypeRegistration(keyToResolve.Type);

            return registrationResult.Resolve(this, keyToResolve, resolutionPath);
        }

        private object CreateObject(Type type, ResolutionList resolutionPath, RegistrationKey keyToResolve)
        {
            var ctors = type.GetConstructors();
            if (ctors.Length == 0)
                ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            Debug.Assert(ctors.Length > 0, "Class must have a constructor!");

            int maxParamCount = ctors.Max(ctor => ctor.GetParameters().Length);
            var maxParamCountCtors = ctors.Where(ctor => ctor.GetParameters().Length == maxParamCount).ToArray();

            object obj;
            if (maxParamCountCtors.Length == 1)
            {
                ConstructorInfo ctor = maxParamCountCtors[0];
                if (resolutionPath.Contains(type))
                    throw new ObjectContainerException("Circular dependency found! " + type.FullName, resolutionPath);

                var args = ResolveArguments(ctor.GetParameters(), keyToResolve, resolutionPath.AddToEnd(type));
                obj = ctor.Invoke(args);
            }
            else
            {
                throw new ObjectContainerException("Multiple public constructors with same maximum parameter count are not supported! " + type.FullName, resolutionPath);
            }

            return obj;
        }

        private object[] ResolveArguments(IEnumerable<ParameterInfo> parameters, RegistrationKey keyToResolve, ResolutionList resolutionPath)
        {
            return parameters.Select(p => IsRegisteredNameParameter(p) ? ResolveRegisteredName(keyToResolve) : Resolve(p.ParameterType, resolutionPath, null)).ToArray();
        }

        private object ResolveRegisteredName(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name;
        }

        private bool IsRegisteredNameParameter(ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType == typeof (string) &&
                   parameterInfo.Name.Equals(REGISTERED_NAME_PARAMETER_NAME);
        }

        #endregion

        public override string ToString()
        {
            return string.Join(Environment.NewLine,
                registrations
                    .Where(r => !(r.Value is NamedInstanceDictionaryRegistration))
                    .Select(r => string.Format("{0} -> {1}", r.Key, (r.Key.Type == typeof(IObjectContainer) && r.Key.Name == null) ? "<self>" : r.Value.ToString())));
        }

        private void AssertNotDisposed()
        {
            if (isDisposed)
                throw new ObjectContainerException("Object container disposed", null);
        }

        public void Dispose()
        {
            isDisposed = true;

            foreach (var obj in objectPool.Values.OfType<IDisposable>().Where(o => !ReferenceEquals(o, this)))
                obj.Dispose();

            objectPool.Clear();
            registrations.Clear();
            resolvedObjects.Clear();
        }
    }

    #region Configuration handling
#if !BODI_LIMITEDRUNTIME

    public class BoDiConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        [ConfigurationCollection(typeof(ContainerRegistrationCollection), AddItemName = "register")]
        public ContainerRegistrationCollection Registrations
        {
            get { return (ContainerRegistrationCollection)this[""]; }
            set { this[""] = value; }
        }
    }

    public class ContainerRegistrationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ContainerRegistrationConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var registrationConfigElement = ((ContainerRegistrationConfigElement)element);
            string elementKey = registrationConfigElement.Interface;
            if (registrationConfigElement.Name != null)
                elementKey = elementKey + "/" + registrationConfigElement.Name;
            return elementKey;
        }

        public void Add(string implementationType, string interfaceType, string name = null)
        {
            BaseAdd(new ContainerRegistrationConfigElement
                        {
                            Implementation = implementationType,
                            Interface = interfaceType,
                            Name = name
                        });
        }
    }

    public class ContainerRegistrationConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("as", IsRequired = true)]
        public string Interface
        {
            get { return (string)this["as"]; }
            set { this["as"] = value; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Implementation
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = false, DefaultValue = null)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
    }

#endif
    #endregion
}