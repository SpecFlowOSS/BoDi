using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace MiniDi
{
    [Serializable]
    public class ObjectContainerException : Exception
    {
        public ObjectContainerException()
        {
        }

        public ObjectContainerException(string message) : base(message)
        {
        }

        public ObjectContainerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ObjectContainerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public interface IObjectContainer
    {
        /// <summary>
        /// Registeres a type as the desired implementation type of an interface.
        /// </summary>
        /// <typeparam name="TType">Implementation type</typeparam>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref name="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overriden before the first resolution for the <typeparamref name="TInterface"/>.</para>
        /// </remarks>
        void RegisterTypeAs<TType, TInterface>() where TType : class, TInterface;
        /// <summary>
        /// Registeres an instance 
        /// </summary>
        /// <typeparam name="TInterface">Interface will be resolved</typeparam>
        /// <param name="instance">The instance implements the interface.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is null.</exception>
        /// <exception cref="ObjectContainerException">If there was already a resolve for the <typeparamref name="TInterface"/>.</exception>
        /// <remarks>
        ///     <para>Previous registrations can be overriden before the first resolution for the <typeparamref name="TInterface"/>.</para>
        ///     <para>The instance will be registered in the object pool, so if a <see cref="Resolve{T}"/> (for another interface) would require an instance of the dynamic type of the <paramref name="instance"/>, the <paramref name="instance"/> will be returned.</para>
        /// </remarks>
        void RegisterInstanceAs<TInterface>(TInterface instance) where TInterface : class;

        /// <summary>
        /// Resolves an implementation object for an interface or type.
        /// </summary>
        /// <typeparam name="T">The interface or type.</typeparam>
        /// <returns>An object implementing <typeparamref name="T"/>.</returns>
        /// <remarks>
        ///     <para>The container pools the objects, so if the interface is resolved twice or the same type is registered for multiple interfaces, a single instance is created and returned.</para>
        /// </remarks>
        T Resolve<T>();
    }

    public class ObjectContainer : IObjectContainer
    {
        private readonly Dictionary<Type, Type> typeRegistrations = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> instanceRegistrations = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> resolvedObjects = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> objectPool = new Dictionary<Type, object>();

        public ObjectContainer()
        {
            RegisterInstanceAs<IObjectContainer>(this);
        }

        public void RegisterTypeAs<TType, TInterface>() where TType : class, TInterface
        {
            AssertNotResolved<TInterface>();

            ClearRegistrations(typeof(TInterface));
            typeRegistrations[typeof(TInterface)] = typeof(TType);
        }

        public void RegisterInstanceAs<TInterface>(TInterface instance) where TInterface : class
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            AssertNotResolved<TInterface>();

            ClearRegistrations(typeof(TInterface));
            instanceRegistrations[typeof(TInterface)] = instance;
            objectPool[instance.GetType()] = instance;
        }

        private void AssertNotResolved<TInterface>()
        {
            if (resolvedObjects.ContainsKey(typeof(TInterface)))
                throw new ObjectContainerException("An object have been resolved for this interface already.");
        }

        private void ClearRegistrations(Type interfaceType)
        {
            typeRegistrations.Remove(interfaceType);
            instanceRegistrations.Remove(interfaceType);
        }

        public T Resolve<T>()
        {
            Type typeToResolve = typeof(T);

            object resolvedObject = Resolve(typeToResolve);

            return (T)resolvedObject;
        }

        private object Resolve(Type typeToResolve)
        {
            object resolvedObject;
            if (!resolvedObjects.TryGetValue(typeToResolve, out resolvedObject))
            {
                resolvedObject = CreateObjectFor(typeToResolve);
                resolvedObjects.Add(typeToResolve, resolvedObject);
            }
            Debug.Assert(typeToResolve.IsInstanceOfType(resolvedObject));
            return resolvedObject;
        }

        private object CreateObjectFor(Type typeToResolve)
        {
            object obj;
            if (instanceRegistrations.TryGetValue(typeToResolve, out obj))
            {
                return obj;
            }

            Type registeredType;
            if (!typeRegistrations.TryGetValue(typeToResolve, out registeredType))
            {
                if (typeToResolve.IsInterface)
                    throw new ObjectContainerException("Interface cannot be resolved: " + typeToResolve.FullName);

                registeredType = typeToResolve;
            }

            if (!objectPool.TryGetValue(registeredType, out obj))
            {
                obj = CreateObject(registeredType);
                objectPool.Add(registeredType, obj);
            }

            return obj;
        }

        private object CreateObject(Type type)
        {
            var ctors = type.GetConstructors();

            object obj;
            if (ctors.Length == 1)
            {
                ConstructorInfo ctor = ctors[0];
                var args = ResolveArguments(ctor.GetParameters());
                obj = ctor.Invoke(args);
            }
            else if (ctors.Length == 0)
            {
                throw new ObjectContainerException("Class must have a public constructor! " + type.FullName);
            }
            else
            {
                throw new ObjectContainerException("Multiple public constructors are not supported! " + type.FullName);
            }

            return obj;
        }

        private object[] ResolveArguments(IEnumerable<ParameterInfo> parameters)
        {
            return parameters.Select(p => Resolve(p.ParameterType)).ToArray();
        }
    }
}