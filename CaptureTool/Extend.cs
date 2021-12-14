using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CaptureTool
{
    public static class Extend
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            try
            {
                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                return bitmapSource;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        public static IntPtr GetHandle(this Window window)
        {
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(window);
            return helper.Handle;
        }

        public static bool AddOwnerChildren(this FrameworkElement owner, UIElement child)
        {
            var parent = (dynamic)owner.Parent;
            Type type = parent.GetType();

            Type panelType = typeof(Panel);
            if (type.IsSubclassOf(panelType))
            {
                parent.Children.Add(child);
                return true;
            }

            PropertyInfo propertyInfo = type.GetProperty("Children");
            if (propertyInfo == null)
            {
                return false;
            }
            else
            {
                parent.Children.Add(child);
                return true;
            }
        }

        private static T CustomDefault<T>()
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)string.Empty;
            }
            else if (typeof(T) == typeof(string[]))
            {
                return (T)(object)new string[0];
            }
            else
            {
                return default(T);
            }
        }

        public static V GetOrDefault<K, V>(this Dictionary<K, V> keyValuePairs, K key)
        {
            if (keyValuePairs != null)
            {
                if (keyValuePairs.TryGetValue(key, out V value))
                {
                    if (value != null)
                    {
                        return value;
                    }
                }
            }

            return CustomDefault<V>();
        }

        public static bool TryAdd<K, V>(this Dictionary<K, V> keyValuePairs, K key, V value)
        {
            if (keyValuePairs.TryGetValue(key, out V gotValue))
            {
                return false;
            }
            else
            {
                keyValuePairs.Add(key, value);
                return true;
            }
        }

        public static T GetOrDefault<T>(this IEnumerable<T> elements, int index)
        {
            if (elements != null)
            {
                if (index < elements.Count() && index > -1)
                {
                    T value = elements.ElementAt(index);
                    if (value != null)
                    {
                        return value;
                    }
                }
            }

            return CustomDefault<T>();
        }
    }
}
