﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FTServer.Models;

namespace FTServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> About(string q)
        {

            var m = new AboutModel();
            q = q.Replace("<", "").Replace(">", "").Trim();

            m.Result = new ResultPartialModel
            {
                Query = q,
                StartId = null
            };

            bool? isdelete = null;

            if (q.StartsWith("http://") || q.StartsWith("https://"))
            {
                isdelete = false;
            }
            else if (q.StartsWith("delete")
              && (q.Contains("http://") || q.Contains("https://")))
            {
                isdelete = true;
            }
            if (!isdelete.HasValue)
            {
                AboutModel.searchList.Enqueue(q);
                while (AboutModel.searchList.Count > 15)
                {
                    String t;
                    AboutModel.searchList.TryDequeue(out t);
                }
            }
            else
            {
                await IndexAPI.indexTextAsync(q, isdelete.Value);
                AboutModel.urlList.Enqueue(q);
                while (AboutModel.urlList.Count > 3)
                {
                    String t;
                    AboutModel.urlList.TryDequeue(out t);
                }
            }
            return View(m);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Result(String q, String s)
        {
            q = q.Replace("<", "").Replace(">", "").Trim();

            long[] ids = new long[] { long.MaxValue };
            if (s != null)
            {
                String[] ss = s.Trim().Split("_");
                ids = new long[ss.Length];
                for (int i = 0; i < ss.Length; i++)
                {
                    ids[i] = long.Parse(ss[i]);
                }
            }

            var Result = new ResultPartialModel
            {
                Query = q,
                StartId = ids
            };
            return View("ResultPartial", Result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

