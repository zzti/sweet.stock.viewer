﻿using sweet.stock.utility.Extentions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace sweet.stock.viewer.Extentions
{
    public static class ListViewExtention
    {
        public static void ViewList<T>(this ListView listView, IEnumerable<T> modelList)
            where T : class
        {
            if (listView == null || !modelList.IsNotEmpty())
            {
                return;
            }
            var props = typeof(T).GetProperties();

            //设置
            //listView.View = View.Details;
            //listView.FullRowSelect = true;
            listView.Clear();

            //添加列
            Graphics graphics = listView.CreateGraphics();

            foreach (var propertyInfo in props)
            {
                var attr = propertyInfo.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute;

                var columnName = attr == null ? string.Empty : attr.Description;
                var sizeF = graphics.MeasureString(columnName, listView.Font);
                listView.Columns.Add(propertyInfo.Name, columnName, (int)(Math.Ceiling(sizeF.Width) + 10), HorizontalAlignment.Left, 0);
            }

            //添加行
            listView.BeginUpdate();

            foreach (var model in modelList)
            {
                ListViewItem item = null;
                //listView.Items.Add(item);

                //item.ImageIndex = 1;

                for (int i = 0; i < props.Length; i++)
                {
                    var propertyInfo = props[i];

                    var value = propertyInfo.GetValue(model, null) ?? "";
                    if (i == 0)
                    {
                        item = listView.Items.Add(value.ToString(), value.ToString(), 1);
                    }
                    else
                    {
                        item.SubItems.Add(value.ToString());
                    }
                }
            }

            if (listView.Items.Count > 1)
            {
                listView.Items[listView.Items.Count - 1].EnsureVisible();//滚动到最后
            }
            listView.EndUpdate();
        }

        public static void ModifyList<T>(this ListView listView, IEnumerable<T> modelList)
            where T : class
        {
            if (listView == null || !modelList.IsNotEmpty())
            {
                return;
            }
            var props = typeof(T).GetProperties();

            listView.BeginUpdate();

            foreach (var model in modelList)
            {
                ListViewItem item = null;

                for (int i = 0; i < props.Length; i++)
                {
                    var propertyInfo = props[i];

                    var value = propertyInfo.GetValue(model, null) ?? "";
                    if (i == 0)
                    {
                        var key = value.ToString();

                        item = listView.Items.Find(key, false).FirstOrDefault();
                    }
                    else
                    {
                        if (item == null) { break; }

                        item.SubItems[i].Text = value.ToString();
                    }
                }
            }

            listView.EndUpdate();
        }
    }
}