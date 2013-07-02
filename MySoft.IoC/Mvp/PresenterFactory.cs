using Castle.Core.Resource;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using System.Configuration;

namespace MySoft.IoC.Mvp
{
    /// <summary>
    /// Presenter Factory
    /// </summary>
    public sealed class PresenterFactory
    {
        private IWindsorContainer container;
        private PresenterFactory()
        {
            if (ConfigurationManager.GetSection("mysoft.framework/mvp") != null)
                this.container = new WindsorContainer(new XmlInterpreter(new ConfigResource("mysoft.framework/mvp")));
            else
                this.container = new WindsorContainer();
        }

        private static PresenterFactory singleton = null;

        /// <summary>
        /// Creates this singleton instance.
        /// </summary>
        /// <returns></returns>
        public static PresenterFactory Create()
        {
            if (singleton == null)
            {
                singleton = new PresenterFactory();
            }
            return singleton;
        }

        /// <summary>
        /// Gets the presenter.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns></returns>
        public IPresenterType GetPresenter<IPresenterType>(object view)
            where IPresenterType : IPresenter
        {
            if (container.Kernel.HasComponent(typeof(IPresenterType)))
            {
                IPresenterType _presenter = container.Resolve<IPresenterType>();
                IPresenter presenter = (IPresenter)_presenter;
                object[] models = new object[presenter.TypeOfModels.Length];
                for (int i = 0; i < models.Length; i++)
                {
                    if (container.Kernel.HasComponent(presenter.TypeOfModels[i]))
                        models[i] = container.Resolve(presenter.TypeOfModels[i]);
                    else
                        models[i] = null;
                }
                presenter.BindView(view);
                presenter.BindModels(models);
                return _presenter;
            }

            return default(IPresenterType);
        }
    }
}
