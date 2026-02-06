namespace DDTank.Shared
{
    public interface IMap
    {
        float Gravity { get; }
        float Wind { get; }
        float AirResistance { get; }
        
        bool IsRectangleEmpty(Rectangle rect);
        bool IsOutMap(int x, int y);
        Physics[] FindPhysicalObjects(Rectangle rect, Physics except);

        void Dig(int cx, int cy, Tile surface, Tile border);
    }
}
