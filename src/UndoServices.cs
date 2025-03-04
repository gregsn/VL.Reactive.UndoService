using System.Reactive;
using VL.Core.Import;
using VL.Lib.Reactive;

namespace Reactive.UndoService;

[ProcessNode(HasStateOutput = true)]
public class UndoServices<T> where T : class, IUndoable<T>
{
    private IChannel<T> _input;
    public IChannel<T> Input
    {
        private get => _input;
        set
        {
            if (_input != value)
            {
                if (_input != null)
                {
                    _input.Dispose();
                    Reset();
                }

                _input = value;
                _input.Subscribe(e =>
                {
                    if (_input.LatestAuthor != ServiceName())
                    {
                        if (HasCurrent())
                        {
                            Undos.Push(Current);
                        }

                        Current = e.Copy();

                        Redos.Clear();
                    }
                });

            }
        }
    }

    private IChannel<Unit> _undo;
    public IChannel<Unit> Undo
    {
        private get => _undo;
        set
        {
            if (_undo != value)
            {
                if (_undo != null)
                {
                    _undo.Dispose();
                }

                _undo = value;
                _undo.Subscribe(e =>
                {
                    if (CanUndo())
                    {
                        if (HasCurrent())
                        {
                            Redos.Push(Current);
                        }

                        Current = Undos.Pop().Copy();

                        _input.SetValueAndAuthor(Current, ServiceName());
                    }
                });
            }
        }
    }

    private IChannel<Unit> _redo;
    public IChannel<Unit> Redo
    {
        private get => _undo;
        set
        {
            if (_redo != value)
            {
                if (_redo != null)
                {
                    _redo.Dispose();
                }

                _redo = value;
                _redo.Subscribe(e =>
                {
                    if (CanRedo())
                    {
                        if (HasCurrent())
                        {
                            Undos.Push(Current);
                        }

                        Current = Redos.Pop().Copy();

                        _input.SetValueAndAuthor(Current, ServiceName());
                    }
                });
            }
        }
    }

    // TODO: IMPLEMENT
    //private int _undoLevels = 20;
    //public int UndoLevels
    //{
    //    private get => _undoLevels;
    //    set
    //    {
    //        if (_undoLevels != value)
    //        {
    //            _undoLevels = value;
    //            Reset();
    //        }
    //    }
    //}

    public Stack<T> Undos = new();
    public Stack<T> Redos = new();
    public T? Current = null;



    [Fragment(IsHidden = true)]
    public bool CanRedo() => Redos.Count() > 0;

    [Fragment(IsHidden = true)]
    public bool CanUndo() => Undos.Count() > 0;

    [Fragment(IsHidden = true)]
    public bool HasCurrent() => Current != null;

    [Fragment(IsHidden = true)]
    public string ServiceName() => this.GetType().Name;

    public void Reset()
    {
        Undos = new();
        Redos = new();
        Current = null;
    }
}
