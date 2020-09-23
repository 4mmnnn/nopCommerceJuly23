﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;

namespace Nop.Services
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert to select list
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="enumObj">Enum</param>
        /// <param name="markCurrentAsSelected">Mark current value as selected</param>
        /// <param name="valuesToExclude">Values to exclude</param>
        /// <param name="useLocalization">Localize</param>
        /// <returns>SelectList</returns>
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj,
           bool markCurrentAsSelected = true, int[] valuesToExclude = null, bool useLocalization = true) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("An Enumeration type is required.", nameof(enumObj));

            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var values = from TEnum enumValue in Enum.GetValues(typeof(TEnum))
                         where valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue))
                         select new { ID = Convert.ToInt32(enumValue), Name = useLocalization ? localizationService.GetLocalizedEnum(enumValue).Result : CommonHelper.ConvertEnum(enumValue.ToString()) };
            object selectedValue = null;
            if (markCurrentAsSelected)
                selectedValue = Convert.ToInt32(enumObj);
            return new SelectList(values, "ID", "Name", selectedValue);
        }

        /// <summary>
        /// Convert to select list
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="objList">List of objects</param>
        /// <param name="selector">Selector for name</param>
        /// <returns>SelectList</returns>
        public static SelectList ToSelectList<T>(this T objList, Func<BaseEntity, string> selector) where T : IEnumerable<BaseEntity>
        {
            return new SelectList(objList.Select(p => new { ID = p.Id, Name = selector(p) }), "ID", "Name");
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        public static async Task<IPagedList<T>> ToPagedList<T>(this IQueryable<T> source, int pageIndex, int pageSize, bool getOnlyTotalCount = false)
        {
            var count = await source.CountAsync();

            var data = new List<T>();

            if (!getOnlyTotalCount && source != null)
                data.AddRange(await source.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync());

            return new PagedList<T>(data, pageIndex,
                pageSize, count);
        }
    }
}