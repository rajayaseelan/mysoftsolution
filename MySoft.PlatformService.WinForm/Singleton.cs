using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MySoft.PlatformService.WinForm
{
    /// <summary>
    /// 回调的窗口
    /// </summary>
    public interface ICallbackForm
    {
        void Run(params object[] args);
    }

    /// <summary>
    /// 创建实例Form委托
    /// </summary>
    /// <returns></returns>
    public delegate T CreateFormEventHandler<T>() where T : Form;

    /// <summary>
    /// 多个单例
    /// </summary>
    public class SingletonMul
    {
        private static Dictionary<string, object> dict = new Dictionary<string, object>();
        private static readonly object syncobj = new object();

        /// <summary>
        /// 移除已经失效的窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RemoveDisposedForm<T>()
            where T : Form
        {
            lock (syncobj)
            {
                foreach (var key in new List<string>(dict.Keys))
                {
                    if (dict[key] == null)
                    {
                        dict.Remove(key);
                        continue;
                    }

                    var winform = dict[key] as WinForm<T>;
                    if (winform == null || winform.IsDisposed)
                    {
                        dict.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// 多单例显示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="form"></param>
        public static void Show<T>(string key, CreateFormEventHandler<T> form)
            where T : Form
        {
            lock (syncobj)
            {
                if (dict.ContainsKey(key))
                {
                    var winform = dict[key] as WinForm<T>;
                    if (winform.IsDisposed)
                    {
                        winform = new WinForm<T>(form());
                        dict[key] = winform;
                    }

                    winform.Show();
                }
                else
                {
                    var t = new WinForm<T>(form());
                    dict.Add(key, t);

                    t.Show();
                }
            }
        }

        /// <summary>
        /// 如果存在Key的窗口，则执行Run方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="args"></param>
        public static void Run<T>(string key, params object[] args)
            where T : Form
        {
            if (ExistForm(key))
            {
                var winform = dict[key] as WinForm<T>;
                if (!winform.IsDisposed && winform.Form is ICallbackForm)
                {
                    (winform.Form as ICallbackForm).Run(args);
                }
            }
        }

        /// <summary>
        /// 判断是否存在窗体
        /// </summary>
        /// <param name="key"></param>
        public static bool ExistForm(string key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// 移除指定Key
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveKey(string key)
        {
            lock (syncobj)
            {
                if (dict.ContainsKey(key))
                {
                    dict.Remove(key);
                }
            }
        }

        /// <summary>
        /// 更新Key
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        public static void RenameKey(string oldKey, string newKey)
        {
            lock (syncobj)
            {
                if (dict.ContainsKey(oldKey))
                {
                    dict[newKey] = dict[oldKey];
                    dict.Remove(oldKey);
                }
            }
        }

        /// <summary>
        /// 隐藏窗体
        /// </summary>
        /// <param name="form"></param>
        public static void Hide<T>(string key)
            where T : Form
        {
            lock (syncobj)
            {
                if (dict.ContainsKey(key))
                {
                    (dict[key] as WinForm<T>).Hide();
                }
            }
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Close<T>(string key)
            where T : Form
        {
            lock (syncobj)
            {
                if (dict.ContainsKey(key))
                {
                    (dict[key] as WinForm<T>).Close();
                    dict.Remove(key);
                }
            }
        }
    }

    /// <summary>
    /// 单例窗口
    /// </summary>
    public class Singleton
    {
        /// <summary>
        /// 显示窗体
        /// </summary>
        public static void Show<T>()
            where T : Form
        {
            if (WinForm<T>.Instance != null)
                WinForm<T>.Instance.Show();
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        /// <param name="form"></param>
        public static void Show<T>(CreateFormEventHandler<T> form)
            where T : Form
        {
            WinForm<T>.GetInstance(form).Show();
        }

        /// <summary>
        /// 显示窗体
        /// </summary>
        /// <param name="form"></param>
        public static void Show<T>(CreateFormEventHandler<T> form, Rectangle rect)
            where T : Form
        {
            WinForm<T>.GetInstance(form).Show(rect);
        }

        /// <summary>
        /// 隐藏窗体
        /// </summary>
        /// <param name="form"></param>
        public static void Hide<T>()
            where T : Form
        {
            if (WinForm<T>.Instance != null)
                WinForm<T>.Instance.Hide();
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Close<T>()
            where T : Form
        {
            if (WinForm<T>.Instance != null)
                WinForm<T>.Instance.Close();
        }

        /// <summary>
        /// 获取窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetForm<T>()
            where T : Form
        {
            if (WinForm<T>.Instance == null) return null;
            return WinForm<T>.Instance.Form;
        }
    }

    /// <summary>
    /// WinForm类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class WinForm<T>  //多线程Singleton模式
         where T : Form
    {
        private static volatile WinForm<T> _instance;    //volatile是为了让编译器对此代码编译后的位置不进行调整
        private static object syncobj = new object();    //辅助器，不参与对象构建
        private T form;
        public T Form
        {
            get
            {
                if (IsDisposed) return null;
                return form;
            }
        }

        public WinForm(T form)
        {
            this.form = form;
        }

        /// <summary>
        /// 是否释放
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return form == null || form.IsDisposed;
            }
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public void Show(Rectangle rect)
        {
            this.Show();

            if (rect.X < 1024) form.Left = rect.X;
            if (rect.Top < 768) form.Top = rect.Y;
            if (rect.Width < 1024) form.Width = rect.Width;
            if (rect.Height < 768) form.Height = rect.Height;
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public void Show()
        {
            if (IsDisposed) return;

            if (!form.Visible)
            {
                form.Show();
            }

            //窗体变为正常状态
            if (form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }

            form.Activate();
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public void Hide()
        {
            if (IsDisposed) return;
            form.Hide();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void Close()
        {
            if (IsDisposed) return;
            form.Close();
            form.Dispose();
        }

        /// <summary>
        /// 单例
        /// </summary>
        public static WinForm<T> Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// 单例
        /// </summary>
        public static WinForm<T> GetInstance(CreateFormEventHandler<T> form)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                lock (syncobj)
                {
                    if (_instance == null || _instance.IsDisposed)       //双检查
                    {
                        _instance = new WinForm<T>(form());
                    }
                }
            }

            return _instance;
        }
    }
}
