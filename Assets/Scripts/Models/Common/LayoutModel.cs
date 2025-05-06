using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class LayoutModel
{
    public PbLayout DataPbLayout = new();
}
public class PbLayout : IProto
{
    private const string 
        _COL = "col",
        _ROW = "row",
        _COL_SPAN = "colSpan",
        _ROW_SPAN = "rowSpan";
    
    public int Col, Row, ColSpan, RowSpan;

    private void _Reset()
    {
   
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Col = data[_COL].AsInt;
        Row = data[_ROW].AsInt;
        ColSpan = data[_COL_SPAN].AsInt;
        RowSpan = data[_ROW_SPAN].AsInt;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_COL] = Col,
            [_ROW] = Row,
            [_COL_SPAN] = ColSpan,
            [_ROW_SPAN] = RowSpan
        };
    }
}
