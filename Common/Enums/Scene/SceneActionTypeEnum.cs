namespace EggLink.DanhengServer.Enums.Scene;

public enum SceneActionTypeEnum
{
    Unknown = 0,
    SetGroupProperty,
    ChangeCurrentTargetPuzzle,
    SetGroupPropertyByCopyAnother,
    PropertyValueEqual,
    SetFloorSavedValue,
    CallCurrentTargetPuzzlePropertyAction,
    CallCurrentTargetPuzzlePropertyChanged
}