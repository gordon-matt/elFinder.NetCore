﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace elFinder.NetCore.Models.Commands
{
    internal class ThumbsResponseModel
    {
        public ThumbsResponseModel()
        {
            Images = new Dictionary<string, string>();
        }

        [JsonProperty("images")]
        public Dictionary<string, string> Images { get; }
    }
}