namespace Megalith
{
    public abstract class MegalithSceneControllerBase
    {
        public abstract void OnSceneUpdate();
        public virtual void OnDestroy()
        {

        }
    }

    public abstract class MegalithSceneController<T, Y> : MegalithSceneControllerBase
        where T : ModelBase
        where Y : MegalithSceneViewBase
    {
        public T Model;
        public Y View;

        protected MegalithEditor Megalith { get; set; }

        public virtual void Setup(ModelBase model, MegalithSceneViewBase view, MegalithEditor megalith)
        {
            Megalith = megalith;
            Model = (T)model;
            View = (Y)view;
        }
    }

}
