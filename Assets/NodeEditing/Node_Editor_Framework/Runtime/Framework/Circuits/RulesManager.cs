using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NodeEditing.Node_Editor_Framework.Runtime.Framework.Circuits
{
    public class RulesManager
    {
        private static Dictionary<string, IRule> Rules { get; set; } = new();
        private static Dictionary<Type, IRule> RuleTypes { get; set; } = new();

        private static bool isInitialised = false;
        
        public static void Init()
        {
            Rules.Clear();
            var rules = GetInheritors<IRule>();

            foreach (var rule in rules)
            {
                RuleTypes.Add(rule.GetType(), rule);
                //var newRule = rule.CreateNew();
                Rules.Add(rule.TypeName, rule);
            }

            isInitialised = true;
        }

        public static IRule CreateRule(Type type)
        {
            if (!isInitialised)
                Init();
            
            var ret = RuleTypes[type].CreateNew();
            return ret;
        }

        public static T GetAttribute<T>(Type type) where T : class
        {
            var ret = type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            return ret;
        }

        public static IEnumerable<Type> GetAllTypes<T>()
        {
            var name = typeof(T).FullName;
            var ret = AppDomain.CurrentDomain.GetAssemblies().SelectMany(d => d.GetTypes().Where(d =>
                d.GetInterfaces().Any(i => i.FullName == name
                )
                && !d.IsInterface && !d.IsAbstract)
            );
            return ret;
        }
        public static IEnumerable<Type> GetInheritorsByName<T>()
        {
            var ret = AppDomain.CurrentDomain.GetAssemblies().SelectMany(d => d.GetTypes().Where(d => d.GetCustomAttributes(typeof(T), false).Any()));
            return ret;
        }

        public static IEnumerable<Type> GetAttributeImplementers<TAttribute>(Assembly ass, params Assembly[] additionalAss) where TAttribute : Attribute
        {
            var ret = ass.GetTypes().Where(d => d.GetCustomAttributes(typeof(TAttribute), false).Any()).ToList();
            var addition = additionalAss.SelectMany(a => a.GetTypes().Where(d => d.GetCustomAttributes(typeof(TAttribute), false).Any()).Select(d => d));
            ret.AddRange(addition);
            return ret;
        }
        //public static List<Type> GetAttributeImplementers<TAttribute>(Assembly ass, params Type[] additionalTypes) where TAttribute : Attribute
        //{
        //    var additional = additionalTypes.Where(d=> d.Assembly != ass).Select(d => d.Assembly).Distinct().ToArray();
        //    return ass.GetAttributeImplementers<TAttribute>(additional);
        //}

        public static IEnumerable<Type> GetInheritorsByName<T>(Assembly ass)
        {
            //is needed because ILinkJobs can run it separate LoadContexts so type equality fails
            var name = typeof(T).FullName;
            var ret = ass.GetTypes().Where(d =>
                d.GetInterfaces().Any(i => i.FullName == name
                )
                && !d.IsInterface && !d.IsAbstract).ToArray();
            return ret;
        }

        public static T[] GetInterfaceImplementers<T>(Assembly asm) where T : class
        {
            var objects = GetInheritors(asm, typeof(T));
            return objects.Select(d => Activator.CreateInstance(d) as T).ToArray();
        }

        public static T[] GetInterfaceImplementers<T>(Assembly asm, Type interfaceType) where T : class
        {
            var ret = GetInheritors(asm, interfaceType).Select(d => Activator.CreateInstance(d) as T).ToArray();
            return ret;
            //var ret = asm.GetInheritors(interfaceType).Select(d => (Activator.CreateInstance(d.MakeGenericType(d.BaseType.GenericTypeArguments[0])))).ToArray();

        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static Type[] GetInheritors(Assembly asm, Type interfaceType)
        {
            var types = GetLoadableTypes(asm);
            var ret = types.Where(d =>
                (interfaceType.IsAssignableFrom(d)
                 || d.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType
                 ))
                && !d.IsInterface && !d.IsAbstract).ToArray();
            return ret;
        }

        public static IEnumerable<T> GetInheritors<T>(Assembly asm) where T : class
        {
            return GetInheritors(asm, typeof(T)).Select(d => Activator.CreateInstance(d) as T);
        }


        public static IEnumerable<T> GetInheritors<T>() where T : class
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetInheritors<T>);
        }

    }
}