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

    public class ObjectContainer
    {
        private readonly Dictionary<Type, Type> typeRegistrations = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> resolvedObjects = new Dictionary<Type, object>();

        public void RegisterTypeAs<TType, TInterface>()
        {
            typeRegistrations.Add(typeof(TInterface), typeof(TType));
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
            Type registeredType;
            if (!typeRegistrations.TryGetValue(typeToResolve, out registeredType))
            {
                if (typeToResolve.IsInterface)
                    throw new ObjectContainerException("Interface cannot be resolved: " + typeToResolve.FullName);

                registeredType = typeToResolve;
            }

            return CreateObject(registeredType);
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

        public void RegisterInstanceAs<T>(T instance)
        {
            resolvedObjects[typeof(T)] = instance;
        }
    }
}