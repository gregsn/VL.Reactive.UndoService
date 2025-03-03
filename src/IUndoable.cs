namespace Reactive.UndoService;

public interface IUndoable<T> where T : class
{
    T Copy();
}
