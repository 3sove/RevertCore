using System;

namespace Revert.Core.Common.Modules
{
    public abstract class FunctionalModule<TSelf> where TSelf : FunctionalModule<TSelf>, new()
    {
        public static TSelf Instance { get; set; } = new TSelf();
    }

    public abstract class FunctionalModule<TSelf, TModel> : FunctionalModule<TSelf> where TSelf : FunctionalModule<TSelf, TModel>, new()
                                                                                    where TModel : ModuleModel
    {
        public TModel Execute(TModel model)
        {
            Model = model;
            Execute();
            return Model;
        }

        protected abstract void Execute();

        public virtual TModel Model { get; set; }

        public void PublishUpdateMessage(TModel model, string message, params object[] args)
        {
            model.UpdateMessageAction(string.Format(message, args));
        }

        public void PublishUpdateMessage(string message, params object[] args)
        {
            if (Equals(Model, null)) Console.WriteLine(message, args);
            Model?.UpdateMessageAction(string.Format(message, args));
        }

        public void PublishUpdateMessage(TModel model, string message)
        {
            model.UpdateMessageAction(message);
        }
    }
}
