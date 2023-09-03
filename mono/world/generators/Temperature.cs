using System;

public partial class Temperature : WorldGenerator
{
    private int _equatorLine;



    public override float GetValueAt(int x, int y)
    {
        return GetNormalizedDistanceToEquator(y);
    }

    public float GetNormalizedDistanceToEquator(int y) => (float) Math.Abs(y - _equatorLine) / _equatorLine;

    // getters & setters
    public int GetParameterEquatorLine() => _equatorLine;
    public void SetParameterEquatorLine(int value) => _equatorLine = value;

}
