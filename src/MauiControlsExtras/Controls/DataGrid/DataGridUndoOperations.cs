namespace MauiControlsExtras.Controls;

/// <summary>
/// Represents an undoable operation in the data grid.
/// </summary>
public interface IUndoableOperation
{
    /// <summary>
    /// Gets a description of this operation.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Undoes this operation.
    /// </summary>
    void Undo();

    /// <summary>
    /// Redoes this operation.
    /// </summary>
    void Redo();
}

/// <summary>
/// Represents an undoable cell edit operation.
/// </summary>
public class CellEditOperation : IUndoableOperation
{
    private readonly DataGridView _grid;
    private readonly object _item;
    private readonly DataGridColumn _column;
    private readonly int _rowIndex;
    private readonly int _columnIndex;
    private readonly object? _oldValue;
    private readonly object? _newValue;

    /// <inheritdoc />
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of CellEditOperation.
    /// </summary>
    public CellEditOperation(
        DataGridView grid,
        object item,
        DataGridColumn column,
        int rowIndex,
        int columnIndex,
        object? oldValue,
        object? newValue)
    {
        _grid = grid;
        _item = item;
        _column = column;
        _rowIndex = rowIndex;
        _columnIndex = columnIndex;
        _oldValue = oldValue;
        _newValue = newValue;
        Description = $"Edit {column.Header}";
    }

    /// <inheritdoc />
    public void Undo()
    {
        _column.SetCellValue(_item, _oldValue);
        _grid.RefreshData();
    }

    /// <inheritdoc />
    public void Redo()
    {
        _column.SetCellValue(_item, _newValue);
        _grid.RefreshData();
    }
}

/// <summary>
/// Represents a batch of undoable operations.
/// </summary>
public class BatchOperation : IUndoableOperation
{
    private readonly List<IUndoableOperation> _operations;

    /// <inheritdoc />
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of BatchOperation.
    /// </summary>
    public BatchOperation(string? description, List<IUndoableOperation> operations)
    {
        Description = description ?? "Multiple changes";
        _operations = operations;
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Undo in reverse order
        for (int i = _operations.Count - 1; i >= 0; i--)
        {
            _operations[i].Undo();
        }
    }

    /// <inheritdoc />
    public void Redo()
    {
        // Redo in forward order
        foreach (var operation in _operations)
        {
            operation.Redo();
        }
    }
}

/// <summary>
/// Represents an undoable row delete operation.
/// </summary>
public class RowDeleteOperation : IUndoableOperation
{
    private readonly DataGridView _grid;
    private readonly object _item;
    private readonly int _index;
    private readonly Action<object, int> _insertAction;
    private readonly Action<object> _removeAction;

    /// <inheritdoc />
    public string Description => "Delete row";

    /// <summary>
    /// Initializes a new instance of RowDeleteOperation.
    /// </summary>
    public RowDeleteOperation(
        DataGridView grid,
        object item,
        int index,
        Action<object, int> insertAction,
        Action<object> removeAction)
    {
        _grid = grid;
        _item = item;
        _index = index;
        _insertAction = insertAction;
        _removeAction = removeAction;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _insertAction(_item, _index);
        _grid.RefreshData();
    }

    /// <inheritdoc />
    public void Redo()
    {
        _removeAction(_item);
        _grid.RefreshData();
    }
}

/// <summary>
/// Represents an undoable paste operation.
/// </summary>
public class PasteOperation : IUndoableOperation
{
    private readonly List<CellEditOperation> _cellEdits;

    /// <inheritdoc />
    public string Description => "Paste";

    /// <summary>
    /// Initializes a new instance of PasteOperation.
    /// </summary>
    public PasteOperation(List<CellEditOperation> cellEdits)
    {
        _cellEdits = cellEdits;
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Undo in reverse order
        for (int i = _cellEdits.Count - 1; i >= 0; i--)
        {
            _cellEdits[i].Undo();
        }
    }

    /// <inheritdoc />
    public void Redo()
    {
        // Redo in forward order
        foreach (var edit in _cellEdits)
        {
            edit.Redo();
        }
    }
}
