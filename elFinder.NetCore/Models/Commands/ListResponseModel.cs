﻿namespace elFinder.NetCore.Models.Commands;

public class ListResponseModel
{
    public ListResponseModel()
    {
        List = new List<string>();
    }

    [JsonPropertyName("list")]
    public List<string> List { get; private set; }
}