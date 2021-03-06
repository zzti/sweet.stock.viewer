﻿using sweet.stock.core.Contract;
using sweet.stock.core.Model;
using sweet.stock.utility;
using sweet.stock.utility.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace sweet.stock.client
{
    public class SinaStockClient : IStockReader
    {
        public List<SuggestInfo> Suggest(string input)
        {
            string url = @"http://suggest3.sinajs.cn/suggest/type=11,12,13,14,15&key=" + input;

            string response = HttpUtil.Get(url, null, Encoding.GetEncoding("GBK"));

            return ParseSuggestInfo(response);
        }

        public List<StockInfo> MarketPrice(string[] stockIds)
        {
            if (!stockIds.IsNotEmpty())
            {
                return new List<StockInfo>();
            }

            string url = @"http://hq.sinajs.cn/list=" + string.Join(",", stockIds);

            string response = HttpUtil.Get(url, null, Encoding.GetEncoding("GBK"));

            return ParseStockInfo(response);
        }

        private List<SuggestInfo> ParseSuggestInfo(string input)
        {
            if (string.IsNullOrEmpty(input)) { return new List<SuggestInfo>(); }

            var match = Regex.Match(input, "=\"(.+)\"");

            if (match.Success)
            {
                string strStockList = match.Groups[1].Value;

                var arr = strStockList.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                var result = new List<SuggestInfo>();
                foreach (var s in arr)
                {
                    var strItem = s.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (strItem.Length == 6)
                    {
                        result.Add(new SuggestInfo() { StockId = strItem[3], StockCode = strItem[2], StockName = strItem[4] });
                    }
                }

                return result;
            }

            return new List<SuggestInfo>();
        }

        private List<StockInfo> ParseStockInfo(string input)
        {
            var result = new List<StockInfo>();
            using (var reader = new StringReader(input))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    string stockId = string.Empty;

                    if (string.IsNullOrEmpty(line)) { break; }

                    var matchStockId = Regex.Match(line, @"hq_str_(\w+)");
                    if (matchStockId.Success)
                    {
                        stockId = matchStockId.Groups[1].Value;
                    }

                    var matchStockContent = Regex.Match(line, "\"(.+)\";");
                    if (matchStockContent.Success)
                    {
                        var strItem = matchStockContent.Groups[1].Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        var model = new StockInfo
                        {
                            StockId = stockId,
                            StockCode = Regex.Match(stockId, @"(\d+)").Groups[1].Value,
                            StockName = strItem[0],
                            OpeningPrice = decimal.Parse(strItem[1]),
                            ClosingPrice = decimal.Parse(strItem[2]),
                            PresentPrice = decimal.Parse(strItem[3]),
                            HighestPrice = decimal.Parse(strItem[4]),
                            LowestPrice = decimal.Parse(strItem[5]),
                            TradingQuantity = double.Parse(strItem[8]),
                            TradingAmount = decimal.Parse(strItem[9]),
                        };

                        result.Add(model);
                    }
                }
            }

            return result;
        }
    }
}