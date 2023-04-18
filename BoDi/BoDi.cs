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
 * V1.5
 *   - Thread-safe object resolution
 *   - New 'instance per dependency' strategy added for type and factory registrations (by MKMZ)
 * 
 * v1.4
 *   - Provide .NET Standard 2.0 and .NET 4.5 library package (#14 by SabotageAndi)
 *   - Fix: Collection was modified issue (#7)
 *   - Exposing BaseContainer to the public interface (#17 by jessicabuttigieg)
 *
 * v1.3
 *   - Fix: When an object resolved without registration using the concrete type it cannot be resolved from sub context
 *   - Added IsRegistered methods to check if an interface or type is already registered (#6)
 *   - Expose the ObjectContainer.RegisterFactoryAs in the IObjectContainer interface (by slawomir-brzezinski-at-interxion)
 *   - eliminate internal TypeHelper class
 *
 * v1.2
 *   - support for mapping of generic type definitions (by ibrahimbensalah)
 *   - object should be created in the parent container, if the registration was applied there
 *   - should be able to customize object creation with a container event (ObjectCreated)
 *   - should be able to register factory delegates
 *   - should be able to retrieve all named instance as a list with container.ResolveAll<T>()
 *   - should not allow resolving value types (structs)
 *   - should list registrations in container ToString()
 *   - should not dispose registered instances by default, disposal can be requested by the 'dispose: true' parameter
 *   - should be able to disable configuration file support (and the dependency on System.Configuration) with BODI_DISABLECONFIGFILESUPPORT compilation symbol
 *   - smaller code refactoring
 *   - improve resolution path handling
 * 
 * v1.1 - released with SpecFlow v1.9.0
 * 
 * 
 * --------------
 * Note about thread safety
 * 
 * BoDi container is not reentrant and can't be used from different threads without further considerations.
 * Typical user (Specflow) ensures it by allocating container per test thread and all feature- and scenario- containers as child containers.
 * The manual synchronization is not necessary for usual cases 
 * (using test-thread, feature or scenario containers and not creating multiple threads from the binding code).
 * Thread-safe object resolution has been introduced to handle the rare cases when dependencies might be resolved from the shared global context concurrently.
 *
 * More information here https://github.com/gasparnagy/BoDi/issues/27
 */
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace BoDi
{
#if !BODI_LIMITEDRUNTIME
    [Serializable]
#endif
    public class ObjectContainerException : Exception
    {
        public ObjectContainerException(string message, Type[] resolutionPath) : base(GetMessage(message, resolutionPath))
        {
        }

#if !BODI_LIMITEDRUNTIME
        protected ObjectContainerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif

        static private string GetMessage(string message, Type[] resolutionPath)
        {
            if (resolutionPath == null || resolutionPath.Length == 0)
                return message;

            return string.Format("{0} (resolution path: {1})", message, string.Join("->", resolutionPath.Select(t => t.FullName).ToArray()));
        }
    }

    public interface IObjectContainer : IDisposable
    {
        /// <summary>
        /// Fired when a new object is created directly by the container. It is not invoked for resolving instance and factory registrations.
        /// </summary>
        event Action<object> ObjectCreated;

        /// <summary>
        /// Registers a type as the desired implementation type of an interface.
        /// </summary>
        /// <typeparam name="TType">Implementation type</typeparam>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <returns>An object which allows to change resolving strategy.</returns>
        /// <param name="name">A name to register named instance, otherwise null.</param>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref name="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overridden before the first resolution for the <typeparamref name="TInterface"/>.</para>
        /// </remarks>
        IStrategyRegistration RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface;

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
        /// Registers an instance produced by <paramref name="factoryDelegate"/>. The delegate will be called only once and the instance it returned will be returned in each resolution.
        /// </summary>
        /// <typeparam name="TInterface">Interface to register as.</typeparam>
        /// <param name="factoryDelegate">The function to run to obtain the instance.</param>
        /// <param name="name">A name to resolve named instance, otherwise null.</param>
        IStrategyRegistration RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null);

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

        /// <summary>
        /// Resolves all implementations of an interface or type.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref name="T"/>.</returns>
        IEnumerable<T> ResolveAll<T>() where T : class;

        /// <summary>
        /// Determines whether the interface or type is registered.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns><c>true</c> if the interface or type is registered; otherwise <c>false</c>.</returns>
        bool IsRegistered<T>();

        /// <summary>
        /// Determines whether the interface or type is registered with the specified name.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the interface or type is registered; otherwise <c>false</c>.</returns>
        bool IsRegistered<T>(string name);
    }
    public interface IContainedInstance
    {
        IObjectContainer Container { get; }
    }
    public interface IStrategyRegistration
    {
        /// <summary>
        /// Changes resolving strategy to a new instance per each dependency.
        /// </summary>
        /// <returns></returns>
        IStrategyRegistration InstancePerDependency();
        /// <summary>
        /// Changes resolving strategy to a single instance per object container. This strategy is a default behaviour. 
        /// </summary>
        /// <returns></returns>
        IStrategyRegistration InstancePerContext();
    }

    public sealed class ObjectContainer : IObjectContainer
    {
        private const string REGISTERED_NAME_PARAMETER_NAME = "registeredName";
        private const string DISABLE_THREAD_SAFE_RESOLUTION = "DISABLE_THREAD_SAFE_RESOLUTION";

        /// <summary>
        /// A very simple immutable linked list of <see cref="Type"/>.
        /// </summary>
        private sealed class ResolutionList
        {
            private readonly RegistrationKey currentRegistrationKey;
            private readonly Type currentResolvedType;
            private readonly ResolutionList nextNode;

            private ResolutionList(RegistrationKey currentRegistrationKey, Type currentResolvedType, ResolutionList nextNode)
            {
                this.currentRegistrationKey = currentRegistrationKey;
                this.currentResolvedType = currentResolvedType;
                this.nextNode = nextNode;
            }

            public bool Contains(RegistrationKey registrationKey)
            {
                var node = this;
                while (node != null)
                {
                    if (node.currentRegistrationKey.Equals(registrationKey))
                    {
                        return true;
                    }
                    node = node.nextNode;
                }

                return false;
            }

            private IEnumerable<KeyValuePair<RegistrationKey, Type>> GetReverseEnumerable()
            {
                var node = this;
                while (node != null)
                {
                    yield return new KeyValuePair<RegistrationKey, Type>(node.currentRegistrationKey, node.currentResolvedType);
                    node = node.nextNode;
                }
            }

            public override string ToString()
            {
                return string.Join(",", GetReverseEnumerable().Select(n => string.Format("{0}:{1}", n.Key, n.Value)));
            }

            public static ResolutionList AddToEnd(ResolutionList node, RegistrationKey registrationKey, Type resolvedType)
            {
                return new ResolutionList(registrationKey, resolvedType, node);
            }

            public static Type[] ToTypeList(ResolutionList node)
            {
                if (node is null)
                {
                    return Type.EmptyTypes;
                }

                return node.GetReverseEnumerable().Select(i => i.Value ?? i.Key.Type).Reverse().ToArray();
            }
        }

        private readonly struct RegistrationKey : IEquatable<RegistrationKey>
        {
            public readonly Type Type;
            private readonly Type typeGroup;
            public readonly string Name;

            public RegistrationKey(Type type, string name)
            {
                if (type is null)
                {
                    ThrowNullException();
                }

                Type = type;
                typeGroup = (type.IsGenericType && !type.IsGenericTypeDefinition) ? type.GetGenericTypeDefinition() : type;
                Name = name;

                void ThrowNullException()
                {
                    throw new ArgumentNullException(nameof(type));
                }
            }

            public override string ToString()
            {
                Debug.Assert(Type.FullName != null);
                if (Name == null)
                    return Type.FullName;

                return string.Format("{0}('{1}')", Type.FullName, Name);
            }

            public bool Equals(RegistrationKey other)
            {
                var isInvertable = other.Type == Type || other.typeGroup == Type || other.Type == typeGroup;
                return isInvertable && String.Equals(other.Name, Name, StringComparison.CurrentCultureIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof(RegistrationKey)) return false;
                return Equals((RegistrationKey)obj);
            }

            public override int GetHashCode()
            {
                return typeGroup.GetHashCode();
            }
        }

        #region Registration types

        private enum SolvingStrategy
        {
            PerContext,
            PerDependency
        }

        private interface IRegistration
        {
            object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);
        }

        private sealed class TypeRegistration : RegistrationWithStrategy, IRegistration
        {
            private readonly Type implementationType;

            public TypeRegistration(Type implementationType)
            {
                this.implementationType = implementationType;
            }

            protected override object ResolvePerContext(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToConstruct = GetTypeToConstruct(keyToResolve);
                var pooledObjectKey = new RegistrationKey(typeToConstruct, keyToResolve.Name);

                if (container.TryGetObjectFromPool(pooledObjectKey, out var obj))
                {
                    return obj;
                }

                if (ObjectContainer.DisableThreadSafeResolution)
                {
                    obj = InternalResolveObject(container, keyToResolve, resolutionPath, typeToConstruct);
                    container.objectPool.Add(pooledObjectKey, obj);
                    return obj;
                }

                if (Monitor.TryEnter(this, ConcurrentObjectResolutionTimeout))
                {
                    try
                    {
                        if (!container.TryGetObjectFromPool(pooledObjectKey, out obj))
                        {
                            obj = InternalResolveObject(container, keyToResolve, resolutionPath, typeToConstruct);
                            container.objectPool.Add(pooledObjectKey, obj);
                            return obj;
                        }
                        return obj;
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }

                throw new ObjectContainerException("Concurrent object resolution timeout (potential circular dependency).", ResolutionList.ToTypeList(resolutionPath));
            }

            protected override object ResolvePerDependency(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                return InternalResolveObject(container, keyToResolve, resolutionPath, GetTypeToConstruct(keyToResolve));
            }

            private static object InternalResolveObject(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath, Type typeToConstruct)
            {
                if (typeToConstruct.IsInterface)
                    ThrowInterfaceResolveException(keyToResolve, resolutionPath);
                return container.CreateObject(typeToConstruct, resolutionPath, keyToResolve);

                void ThrowInterfaceResolveException(RegistrationKey key, ResolutionList path)
                {
                    throw new ObjectContainerException("Interface cannot be resolved: " + key, ResolutionList.ToTypeList(path));
                }
            }

            private Type GetTypeToConstruct(RegistrationKey keyToResolve)
            {
                var targetType = implementationType;
                if (!targetType.IsGenericTypeDefinition)
                {
                    return targetType;
                }
                return targetType.MakeGenericType(keyToResolve.Type.GetGenericArguments());
            }

            public override string ToString()
            {
                return "Type: " + implementationType.FullName;
            }
        }

        private sealed class InstanceRegistration : IRegistration
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

        private abstract class RegistrationWithStrategy : IStrategyRegistration
        {
            protected SolvingStrategy solvingStrategy = SolvingStrategy.PerContext;

            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                if (solvingStrategy == SolvingStrategy.PerDependency)
                {
                    return ResolvePerDependency(container, keyToResolve, resolutionPath);
                }
                return ResolvePerContext(container, keyToResolve, resolutionPath);
            }

            protected abstract object ResolvePerContext(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);
            protected abstract object ResolvePerDependency(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath);

            public IStrategyRegistration InstancePerDependency()
            {
                solvingStrategy = SolvingStrategy.PerDependency;
                return this;
            }

            public IStrategyRegistration InstancePerContext()
            {
                solvingStrategy = SolvingStrategy.PerContext;
                return this;
            }
        }

        private sealed class FactoryRegistration : RegistrationWithStrategy, IRegistration
        {
            private readonly Delegate factoryDelegate;

            public FactoryRegistration(Delegate factoryDelegate)
            {
                this.factoryDelegate = factoryDelegate;
            }

            protected override object ResolvePerContext(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                if (container.TryGetObjectFromPool(keyToResolve, out var obj))
                {
                    return obj;
                }

                if (ObjectContainer.DisableThreadSafeResolution)
                {
                    obj = container.InvokeFactoryDelegate(this.factoryDelegate, resolutionPath, keyToResolve);
                    container.objectPool.Add(keyToResolve, obj);
                    return obj;
                }

                if (Monitor.TryEnter(this, ConcurrentObjectResolutionTimeout))
                {
                    try
                    {
                        if (!container.TryGetObjectFromPool(keyToResolve, out obj))
                        {
                            obj = container.InvokeFactoryDelegate(this.factoryDelegate, resolutionPath, keyToResolve);
                            container.objectPool.Add(keyToResolve, obj);
                            return obj;
                        }
                        return obj;
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }

                throw new ObjectContainerException("Concurrent object resolution timeout (potential circular dependency).", ResolutionList.ToTypeList(resolutionPath));
            }

            protected override object ResolvePerDependency(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                return container.InvokeFactoryDelegate(factoryDelegate, resolutionPath, keyToResolve);
            }
        }

        private sealed class NonDisposableWrapper
        {
            public object Object { get; private set; }

            public NonDisposableWrapper(object obj)
            {
                Object = obj;
            }
        }

        private sealed class NamedInstanceDictionaryRegistration : IRegistration
        {
            public object Resolve(ObjectContainer container, RegistrationKey keyToResolve, ResolutionList resolutionPath)
            {
                var typeToResolve = keyToResolve.Type;
                Debug.Assert(typeToResolve.IsGenericType && typeToResolve.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                var genericArguments = typeToResolve.GetGenericArguments();
                var keyType = genericArguments[0];
                var targetType = genericArguments[1];
                var result = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(genericArguments));

                foreach (var namedRegistration in container.registrations.Where(r => r.Key.Name != null && r.Key.Type == targetType).Select(r => r.Key).ToList())
                {
                    var convertedKey = ChangeType(namedRegistration.Name, keyType);
                    Debug.Assert(convertedKey != null);
                    result.Add(convertedKey, container.Resolve(namedRegistration.Type, namedRegistration.Name));
                }

                return result;
            }

            private static object ChangeType(string name, Type keyType)
            {
                if (keyType.IsEnum)
                    return Enum.Parse(keyType, name, true);

                Debug.Assert(keyType == typeof(string));
                return name;
            }
        }

        #endregion

        private bool isDisposed;
        private readonly ObjectContainer baseContainer;
        private readonly ConcurrentDictionary<RegistrationKey, IRegistration> registrations = new ConcurrentDictionary<RegistrationKey, IRegistration>();
        private readonly HashSet<RegistrationKey> resolvedKeys = new HashSet<RegistrationKey>();
        private readonly Dictionary<RegistrationKey, object> objectPool = new Dictionary<RegistrationKey, object>();

        public static bool DisableThreadSafeResolution { get; set; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(DISABLE_THREAD_SAFE_RESOLUTION));

        public event Action<object> ObjectCreated;
        public IObjectContainer BaseContainer => baseContainer;

        public static TimeSpan ConcurrentObjectResolutionTimeout { get; set; } = TimeSpan.FromSeconds(1);

        public ObjectContainer(IObjectContainer baseContainer = null)
        {
            if (baseContainer != null && !(baseContainer is ObjectContainer))
                throw new ArgumentException("Base container must be an ObjectContainer", nameof(baseContainer));

            this.baseContainer = (ObjectContainer)baseContainer;
            RegisterInstanceAs(this, typeof(IObjectContainer));
        }

        #region Registration

        public IStrategyRegistration RegisterTypeAs<TInterface>(Type implementationType, string name = null) where TInterface : class
        {
            return this.RegisterTypeAs(implementationType, typeof(TInterface), name);
        }

        public IStrategyRegistration RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface
        {
            return this.RegisterTypeAs(typeof(TType), typeof(TInterface), name);
        }

        public IStrategyRegistration RegisterTypeAs(Type implementationType, Type interfaceType)
        {
            if (!IsValidTypeMapping(implementationType, interfaceType))
                throw new InvalidOperationException("type mapping is not valid");
            return RegisterTypeAs(implementationType, interfaceType, null);
        }

        private static bool IsValidTypeMapping(Type implementationType, Type interfaceType)
        {
            if (interfaceType.IsAssignableFrom(implementationType))
                return true;

            if (interfaceType.IsGenericTypeDefinition && implementationType.IsGenericTypeDefinition)
            {
                var baseTypes = GetBaseTypes(implementationType).ToArray();
                return baseTypes.Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType);
            }

            return false;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            if (type.BaseType == null) return type.GetInterfaces();

            return Enumerable.Repeat(type.BaseType, 1)
                             .Concat(type.GetInterfaces())
                             .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                             .Concat(GetBaseTypes(type.BaseType));
        }


        private static RegistrationKey CreateNamedInstanceDictionaryKey(Type targetType)
        {
            return new RegistrationKey(typeof(IDictionary<,>).MakeGenericType(typeof(string), targetType), null);
        }

        private void UpdateRegistration(RegistrationKey key, IRegistration registration)
        {
            registrations[key] = registration;
            AddNamedDictionaryRegistration(key);
        }

        private IRegistration EnsureImplicitRegistration(RegistrationKey key)
        {
            var registration = registrations.GetOrAdd(key, registrationKey => new TypeRegistration(registrationKey.Type));

            AddNamedDictionaryRegistration(key);

            return registration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddNamedDictionaryRegistration(RegistrationKey key)
        {
            if (key.Name != null)
            {
                var dictKey = CreateNamedInstanceDictionaryKey(key.Type);
                registrations.TryAdd(dictKey, new NamedInstanceDictionaryRegistration());
            }
        }

        private IStrategyRegistration RegisterTypeAs(Type implementationType, Type interfaceType, string name)
        {
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            var typeRegistration = new TypeRegistration(implementationType);
            this.UpdateRegistration(registrationKey, typeRegistration);

            return typeRegistration;
        }

        public void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false)
        {
            if (instance is null) ThrowArgumentNullException(nameof(instance));
            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            this.UpdateRegistration(registrationKey, new InstanceRegistration(instance));
            objectPool[new RegistrationKey(instance.GetType(), name)] = GetPoolableInstance(instance, dispose);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object GetPoolableInstance(object instance, bool dispose)
        {
            return !dispose && (instance is IDisposable) ? new NonDisposableWrapper(instance) : instance;
        }

        public void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class
        {
            RegisterInstanceAs(instance, typeof(TInterface), name, dispose);
        }

        public IStrategyRegistration RegisterFactoryAs<TInterface>(Func<TInterface> factoryDelegate, string name = null)
        {
            return RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public IStrategyRegistration RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null)
        {
            return RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public void RegisterFactoryAs<TInterface>(Delegate factoryDelegate, string name = null)
        {
            RegisterFactoryAs(factoryDelegate, typeof(TInterface), name);
        }

        public IStrategyRegistration RegisterFactoryAs(Delegate factoryDelegate, Type interfaceType, string name = null)
        {
            if (factoryDelegate == null) ThrowArgumentNullException(nameof(factoryDelegate));
            if (interfaceType == null) ThrowArgumentNullException(nameof(interfaceType));

            var registrationKey = new RegistrationKey(interfaceType, name);
            AssertNotResolved(registrationKey);

            var factoryRegistration = new FactoryRegistration(factoryDelegate);
            this.UpdateRegistration(registrationKey, factoryRegistration);

            return factoryRegistration;
        }

        private static void ThrowArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }

        public bool IsRegistered<T>()
        {
            return this.IsRegistered<T>(null);
        }

        public bool IsRegistered<T>(string name)
        {
            return this.registrations.ContainsKey(new RegistrationKey(typeof(T), name));
        }

        // ReSharper disable once UnusedParameter.Local
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssertNotResolved(RegistrationKey interfaceType)
        {
            if (resolvedKeys.Contains(interfaceType))
                ThrowObjectAlreadyResolved();

            void ThrowObjectAlreadyResolved()
            {
                throw new ObjectContainerException("An object has been resolved for this interface already.", null);
            }
        }

#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT
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
            return (T)this.Resolve(typeof(T), name);
        }

        public object Resolve(Type typeToResolve, string name = null)
        {
            return Resolve(typeToResolve, null, name);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            foreach (var pair in this.registrations)
            {
                if (pair.Key.Type == typeof(T))
                {
                    yield return (T)Resolve(pair.Key.Type, null, pair.Key.Name);
                }
            }
        }

        private object Resolve(Type typeToResolve, ResolutionList resolutionPath, string name)
        {
            AssertNotDisposed();

            var keyToResolve = new RegistrationKey(typeToResolve, name);
            object resolvedObject = ResolveObject(keyToResolve, resolutionPath);
            resolvedKeys.Add(keyToResolve);
            Debug.Assert(typeToResolve.IsInstanceOfType(resolvedObject));
            return resolvedObject;
        }

        private KeyValuePair<ObjectContainer, IRegistration> GetRegistrationResult(RegistrationKey keyToResolve)
        {
            IRegistration registration;
            if (registrations.TryGetValue(keyToResolve, out registration))
            {
                return new KeyValuePair<ObjectContainer, IRegistration>(this, registration);
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
                return new KeyValuePair<ObjectContainer, IRegistration>(this, new NamedInstanceDictionaryRegistration());
            }

            return new KeyValuePair<ObjectContainer, IRegistration>(this, EnsureImplicitRegistration(keyToResolve));
        }

        private static bool IsDefaultNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) &&
                   keyToResolve.Type.GetGenericArguments()[0] == typeof(string);
        }

        private static bool IsSpecialNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return IsNamedInstanceDictionaryKey(keyToResolve) &&
                   keyToResolve.Type.GetGenericArguments()[0].IsEnum;
        }

        private static bool IsNamedInstanceDictionaryKey(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name == null && keyToResolve.Type.IsGenericType && keyToResolve.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }

        private bool TryGetObjectFromPool(RegistrationKey pooledObjectKey, out object obj)
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
            // All primitive types are structs, no need to check
            if (keyToResolve.Type.IsValueType || keyToResolve.Type == typeof(string))
                throw new ObjectContainerException("Primitive types or structs cannot be resolved: " + keyToResolve.Type.FullName, ResolutionList.ToTypeList(resolutionPath));

            var registrationToUse = GetRegistrationResult(keyToResolve);

            var resolutionPathForResolve = registrationToUse.Key == this ? resolutionPath : null;
            var result = registrationToUse.Value.Resolve(registrationToUse.Key, keyToResolve, resolutionPathForResolve);

            return result;
        }

        private object CreateObject(Type type, ResolutionList resolutionPath, RegistrationKey keyToResolve)
        {
            var ctors = type.GetConstructors();
            if (ctors.Length == 0)
                ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            Debug.Assert(ctors.Length > 0, "Class must have a constructor!");

            int maxLength = -1;
            ConstructorInfo maxCtor = null;
            ParameterInfo[] maxParameterInfos = null;
            bool sameLength = false;
            foreach (var ctor in ctors)
            {
                var parameterInfos = ctor.GetParameters();
                if (maxLength < parameterInfos.Length)
                {
                    maxLength = parameterInfos.Length;
                    maxCtor = ctor;
                    maxParameterInfos = parameterInfos;
                    sameLength = false;
                }
                else if (maxLength == parameterInfos.Length)
                {
                    sameLength = true;
                }
            }

            if (sameLength)
                throw new ObjectContainerException("Multiple public constructors with same maximum parameter count are not supported! " + type.FullName, ResolutionList.ToTypeList(resolutionPath));

            if (resolutionPath != null && resolutionPath.Contains(keyToResolve))
                throw new ObjectContainerException("Circular dependency found! " + type.FullName, ResolutionList.ToTypeList(resolutionPath));

            var args = this.ResolveArguments(maxParameterInfos, keyToResolve, resolutionPath, type);
            var obj = maxCtor.Invoke(args);

            OnObjectCreated(obj);

            return obj;
        }

        private void OnObjectCreated(object obj)
        {
            ObjectCreated?.Invoke(obj);
        }

        private object InvokeFactoryDelegate(Delegate factoryDelegate, ResolutionList resolutionPath, RegistrationKey keyToResolve)
        {
            if (resolutionPath != null && resolutionPath.Contains(keyToResolve))
                throw new ObjectContainerException("Circular dependency found! " + factoryDelegate.ToString(), ResolutionList.ToTypeList(resolutionPath));

            var args = ResolveArguments(factoryDelegate.Method.GetParameters(), keyToResolve, resolutionPath, null);
            return factoryDelegate.DynamicInvoke(args);
        }

        private static readonly object[] NoArgumentArray = new object[0];
        private object[] ResolveArguments(ParameterInfo[] parameters, RegistrationKey keyToResolve, ResolutionList resolutionPath, Type typeToResolve)
        {
            if (parameters.Length == 0)
            {
                return NoArgumentArray;
            }

            resolutionPath = ResolutionList.AddToEnd(resolutionPath, keyToResolve, typeToResolve);

            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                args[i] = IsRegisteredNameParameter(p) ? ResolveRegisteredName(keyToResolve) : Resolve(p.ParameterType, resolutionPath, null);
            }

            return args;
        }

        private static object ResolveRegisteredName(RegistrationKey keyToResolve)
        {
            return keyToResolve.Name;
        }

        private static bool IsRegisteredNameParameter(ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType == typeof(string) &&
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssertNotDisposed()
        {
            if (isDisposed)
                ThrowObjectDisposed();

            void ThrowObjectDisposed()
            {
                throw new ObjectContainerException("Object container disposed", null);
            }
        }

        public void Dispose()
        {
            isDisposed = true;

            foreach (var obj in objectPool.Values)
            {
                if (obj is IDisposable disposable && obj != this)
                {
                    disposable.Dispose();
                }
            }

            objectPool.Clear();
            registrations.Clear();
            resolvedKeys.Clear();
        }
    }

    #region Configuration handling
#if !BODI_LIMITEDRUNTIME && !BODI_DISABLECONFIGFILESUPPORT

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
