using System.Reflection;

namespace HexTecGames.BuildHelper.Tests.Editor
{
    public static class TestUtils
    {
        public static T CallPrivate<T>(object instance, string method, params object[] args)
        {
            var m = instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)m.Invoke(instance, args);
        }

        public static void CallPrivate(object instance, string method, params object[] args)
        {
            var m = instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);
            m.Invoke(instance, args);
        }
    }

}