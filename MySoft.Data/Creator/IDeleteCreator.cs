
namespace MySoft.Data
{
    interface IDeleteCreator<T> : IDeleteCreator
    where T : Entity
    {
    }

    interface IDeleteCreator : IWhereCreator<DeleteCreator>
    {
    }
}
