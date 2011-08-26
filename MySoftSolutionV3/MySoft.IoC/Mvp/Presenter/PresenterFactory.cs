using System;
using System.Reflection;
using MySoft.IoC;

namespace MySoft.IoC
{
    /// <summary>
    /// Presenter Factory
    /// </summary>
    public sealed class PresenterFactory
    {
        private IServiceContainer container;
        private PresenterFactory()
        {
            container = CastleFactory.Create().ServiceContainer;
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
        {
            if (container.Kernel.HasComponent(typeof(IPresenterType)))
            {
                IPresenterType _presenter = (IPresenterType)container[typeof(IPresenterType)];
                if (typeof(IPresenter).IsAssignableFrom(_presenter.GetType()))
                {
                    IPresenter presenter = (IPresenter)_presenter;
                    object[] models = new object[presenter.TypeOfModels.Length];
                    for (int i = 0; i < models.Length; i++)
                    {
                        if (container.Kernel.HasComponent(presenter.TypeOfModels[i]))
                            models[i] = container[presenter.TypeOfModels[i]];
                        else
                            models[i] = null;
                    }
                    presenter.BindView(view);
                    presenter.BindModels(models);
                    return _presenter;
                }
            }

            return default(IPresenterType);
        }
    }
}
