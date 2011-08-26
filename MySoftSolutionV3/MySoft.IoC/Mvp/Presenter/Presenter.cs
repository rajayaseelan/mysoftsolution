using System;

namespace MySoft.IoC
{
    /// <summary>
    /// The extended interface of all presenters
    /// </summary>
    public interface IPresenter
    {
        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        void BindView(object view);
        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        void BindModels(object[] models);
        /// <summary>
        /// Gets the type of view.
        /// </summary>
        /// <value>The type of view.</value>
        Type TypeOfView { get; }
        /// <summary>
        /// Gets the types of model.
        /// </summary>
        /// <value>The types of model.</value>
        Type[] TypeOfModels { get; }
    }

    /// <summary>
    /// Base class of all presenters
    /// </summary>
    /// <typeparam name="ViewType"></typeparam>
    public abstract class Presenter<ViewType> : IPresenter
    {
        /// <summary>
        /// The view
        /// </summary>
        protected ViewType view;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Presenter&lt;ViewType&gt;"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get
            {
                return view != null;
            }
        }

        #region IPresenter2 Members

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void BindView(object view)
        {
            this.view = (ViewType)view;
        }

        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        public void BindModels(object[] models)
        {
            //null is ok
        }

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        public Type TypeOfView
        {
            get
            {
                return typeof(ViewType);
            }
        }

        /// <summary>
        /// Gets the types of the model.
        /// </summary>
        /// <value>The types of the model.</value>
        public Type[] TypeOfModels
        {
            get
            {
                return new Type[0];
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class of all presenters
    /// </summary>
    /// <typeparam name="ViewType"></typeparam>
    /// <typeparam name="IModelType"></typeparam>
    public abstract class Presenter<ViewType, IModelType> : IPresenter
    //where IModelType : IServiceInterface
    {
        /// <summary>
        /// The view
        /// </summary>
        protected ViewType view;

        /// <summary>
        /// The model
        /// </summary>
        protected IModelType model;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Presenter&lt;IViewType, IModelType&gt;"/> is initialized.
        /// Only when both view and model are binded, a presenter is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get
            {
                return view != null && model != null;
            }
        }

        #region IPresenter Members

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void BindView(object view)
        {
            this.view = (ViewType)view;
        }

        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        public void BindModels(object[] models)
        {
            this.model = (IModelType)models[0];
        }

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        public Type TypeOfView
        {
            get
            {
                return typeof(ViewType);
            }
        }

        /// <summary>
        /// Gets the types of the model.
        /// </summary>
        /// <value>The types of the model.</value>
        public Type[] TypeOfModels
        {
            get
            {
                return new Type[] { typeof(IModelType) };
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class of all presenters
    /// </summary>
    /// <typeparam name="ViewType"></typeparam>
    /// <typeparam name="IModelType1"></typeparam>
    /// <typeparam name="IModelType2"></typeparam>
    public abstract class Presenter<ViewType, IModelType1, IModelType2> : IPresenter
    //where IModelType1 : IServiceInterface
    //where IModelType2 : IServiceInterface
    {
        /// <summary>
        /// The view
        /// </summary>
        protected ViewType view;

        /// <summary>
        /// The model 1
        /// </summary>
        protected IModelType1 model1;

        /// <summary>
        /// The model 2
        /// </summary>
        protected IModelType2 model2;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Presenter&lt;ViewType, IModelType1, IModelType2&gt;"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get
            {
                return view != null && model1 != null && model2 != null;
            }
        }

        #region IPresenter2 Members

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void BindView(object view)
        {
            this.view = (ViewType)view;
        }

        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        public void BindModels(object[] models)
        {
            this.model1 = (IModelType1)models[0];
            this.model2 = (IModelType2)models[1];
        }

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        public Type TypeOfView
        {
            get
            {
                return typeof(ViewType);
            }
        }

        /// <summary>
        /// Gets the types of the model.
        /// </summary>
        /// <value>The types of the model.</value>
        public Type[] TypeOfModels
        {
            get
            {
                return new Type[] { typeof(IModelType1), typeof(IModelType2) };
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class of all presenters
    /// </summary>
    /// <typeparam name="ViewType"></typeparam>
    /// <typeparam name="IModelType1"></typeparam>
    /// <typeparam name="IModelType2"></typeparam>
    /// <typeparam name="IModelType3"></typeparam>
    public abstract class Presenter<ViewType, IModelType1, IModelType2, IModelType3> : IPresenter
    //where IModelType1 : IServiceInterface
    //where IModelType2 : IServiceInterface
    //where IModelType3 : IServiceInterface
    {
        /// <summary>
        /// The view
        /// </summary>
        protected ViewType view;

        /// <summary>
        /// The model 1
        /// </summary>
        protected IModelType1 model1;

        /// <summary>
        /// The model 2
        /// </summary>
        protected IModelType2 model2;

        /// <summary>
        /// The model 3
        /// </summary>
        protected IModelType3 model3;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Presenter&lt;ViewType, IModelType1, IModelType2, IModelType3&gt;"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get
            {
                return view != null && model1 != null && model2 != null && model3 != null;
            }
        }

        #region IPresenter2 Members

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void BindView(object view)
        {
            this.view = (ViewType)view;
        }

        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        public void BindModels(object[] models)
        {
            this.model1 = (IModelType1)models[0];
            this.model2 = (IModelType2)models[1];
            this.model3 = (IModelType3)models[2];
        }

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        public Type TypeOfView
        {
            get
            {
                return typeof(ViewType);
            }
        }

        /// <summary>
        /// Gets the types of the model.
        /// </summary>
        /// <value>The types of the model.</value>
        public Type[] TypeOfModels
        {
            get
            {
                return new Type[] { typeof(IModelType1), typeof(IModelType2), typeof(IModelType3) };
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class of all presenters
    /// </summary>
    /// <typeparam name="ViewType"></typeparam>
    /// <typeparam name="IModelType1"></typeparam>
    /// <typeparam name="IModelType2"></typeparam>
    /// <typeparam name="IModelType3"></typeparam>
    /// <typeparam name="IModelType4"></typeparam>
    public abstract class Presenter<ViewType, IModelType1, IModelType2, IModelType3, IModelType4> : IPresenter
    //where IModelType1 : IServiceInterface
    //where IModelType2 : IServiceInterface
    //where IModelType3 : IServiceInterface
    //where IModelType4 : IServiceInterface
    {
        /// <summary>
        /// The view
        /// </summary>
        protected ViewType view;

        /// <summary>
        /// The model 1
        /// </summary>
        protected IModelType1 model1;

        /// <summary>
        /// The model 2
        /// </summary>
        protected IModelType2 model2;

        /// <summary>
        /// The model 3
        /// </summary>
        protected IModelType3 model3;

        /// <summary>
        /// The model 4
        /// </summary>
        protected IModelType4 model4;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Presenter&lt;ViewType, IModelType1, IModelType2, IModelType3, IModelType4&gt;"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get
            {
                return view != null && model1 != null && model2 != null && model3 != null && model4 != null;
            }
        }

        #region IPresenter2 Members

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void BindView(object view)
        {
            this.view = (ViewType)view;
        }

        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        public void BindModels(object[] models)
        {
            this.model1 = (IModelType1)models[0];
            this.model2 = (IModelType2)models[1];
            this.model3 = (IModelType3)models[2];
            this.model4 = (IModelType4)models[3];
        }

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        public Type TypeOfView
        {
            get
            {
                return typeof(ViewType);
            }
        }

        /// <summary>
        /// Gets the types of the model.
        /// </summary>
        /// <value>The types of the model.</value>
        public Type[] TypeOfModels
        {
            get
            {
                return new Type[] { typeof(IModelType1), typeof(IModelType2), typeof(IModelType3), typeof(IModelType4) };
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class of all presenters
    /// </summary>
    /// <typeparam name="ViewType"></typeparam>
    /// <typeparam name="IModelType1"></typeparam>
    /// <typeparam name="IModelType2"></typeparam>
    /// <typeparam name="IModelType3"></typeparam>
    /// <typeparam name="IModelType4"></typeparam>
    /// <typeparam name="IModelType5"></typeparam>
    public abstract class Presenter<ViewType, IModelType1, IModelType2, IModelType3, IModelType4, IModelType5> : IPresenter
    //where IModelType1 : IServiceInterface
    //where IModelType2 : IServiceInterface
    //where IModelType3 : IServiceInterface
    //where IModelType4 : IServiceInterface
    //where IModelType5 : IServiceInterface
    {
        /// <summary>
        /// The view
        /// </summary>
        protected ViewType view;

        /// <summary>
        /// The model 1
        /// </summary>
        protected IModelType1 model1;

        /// <summary>
        /// The model 2
        /// </summary>
        protected IModelType2 model2;

        /// <summary>
        /// The model 3
        /// </summary>
        protected IModelType3 model3;

        /// <summary>
        /// The model 4
        /// </summary>
        protected IModelType4 model4;

        /// <summary>
        /// The model 5
        /// </summary>
        protected IModelType5 model5;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Presenter&lt;ViewType, IModelType1, IModelType2, IModelType3, IModelType4, IModelType5&gt;"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get
            {
                return view != null && model1 != null && model2 != null && model3 != null && model4 != null && model5 != null;
            }
        }

        #region IPresenter2 Members

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void BindView(object view)
        {
            this.view = (ViewType)view;
        }

        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        public void BindModels(object[] models)
        {
            this.model1 = (IModelType1)models[0];
            this.model2 = (IModelType2)models[1];
            this.model3 = (IModelType3)models[2];
            this.model4 = (IModelType4)models[3];
            this.model5 = (IModelType5)models[4];
        }

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        public Type TypeOfView
        {
            get
            {
                return typeof(ViewType);
            }
        }

        /// <summary>
        /// Gets the types of the model.
        /// </summary>
        /// <value>The types of the model.</value>
        public Type[] TypeOfModels
        {
            get
            {
                return new Type[] { typeof(IModelType1), typeof(IModelType2), typeof(IModelType3), typeof(IModelType4), typeof(IModelType5) };
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class of all presenters
    /// </summary>
    /// <typeparam name="ViewType"></typeparam>
    /// <typeparam name="IModelType1"></typeparam>
    /// <typeparam name="IModelType2"></typeparam>
    /// <typeparam name="IModelType3"></typeparam>
    /// <typeparam name="IModelType4"></typeparam>
    /// <typeparam name="IModelType5"></typeparam>
    /// <typeparam name="IModelType6"></typeparam>
    public abstract class Presenter<ViewType, IModelType1, IModelType2, IModelType3, IModelType4, IModelType5, IModelType6> : IPresenter
    //where IModelType1 : IServiceInterface
    //where IModelType2 : IServiceInterface
    //where IModelType3 : IServiceInterface
    //where IModelType4 : IServiceInterface
    //where IModelType5 : IServiceInterface
    //where IModelType6 : IServiceInterface
    {
        /// <summary>
        /// The view
        /// </summary>
        protected ViewType view;

        /// <summary>
        /// The model 1
        /// </summary>
        protected IModelType1 model1;

        /// <summary>
        /// The model 2
        /// </summary>
        protected IModelType2 model2;

        /// <summary>
        /// The model 3
        /// </summary>
        protected IModelType3 model3;

        /// <summary>
        /// The model 4
        /// </summary>
        protected IModelType4 model4;

        /// <summary>
        /// The model 5
        /// </summary>
        protected IModelType5 model5;

        /// <summary>
        /// The model 6
        /// </summary>
        protected IModelType5 model6;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Presenter&lt;ViewType, IModelType1, IModelType2, IModelType3, IModelType4, IModelType5, IModelType6&gt;"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized
        {
            get
            {
                return view != null && model1 != null && model2 != null && model3 != null && model4 != null && model5 != null && model6 != null;
            }
        }

        #region IPresenter2 Members

        /// <summary>
        /// Binds the view.
        /// </summary>
        /// <param name="view">The view.</param>
        public void BindView(object view)
        {
            this.view = (ViewType)view;
        }

        /// <summary>
        /// Binds the models.
        /// </summary>
        /// <param name="model">The models.</param>
        public void BindModels(object[] models)
        {
            this.model1 = (IModelType1)models[0];
            this.model2 = (IModelType2)models[1];
            this.model3 = (IModelType3)models[2];
            this.model4 = (IModelType4)models[3];
            this.model5 = (IModelType5)models[4];
            this.model6 = (IModelType5)models[5];
        }

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        public Type TypeOfView
        {
            get
            {
                return typeof(ViewType);
            }
        }

        /// <summary>
        /// Gets the types of the model.
        /// </summary>
        /// <value>The types of the model.</value>
        public Type[] TypeOfModels
        {
            get
            {
                return new Type[] { typeof(IModelType1), typeof(IModelType2), typeof(IModelType3), typeof(IModelType4), typeof(IModelType5), typeof(IModelType6) };
            }
        }

        #endregion
    }
}
