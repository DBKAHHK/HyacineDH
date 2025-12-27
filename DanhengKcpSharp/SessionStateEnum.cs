namespace EggLink.DanhengServer.Kcp;

public enum SessionStateEnum
{
    INACTIVE,
    WAITING_FOR_TOKEN,
    WAITING_FOR_LOGIN,
    PICKING_CHARACTER,
    ACTIVE
}