﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ViewModels
{
    public class Sample5ViewModel : RedwoodViewModelBase
    {

        public Sample5TestControlViewModel ViewModel1 { get; set; }

        public Sample5TestControlViewModel ViewModel2 { get; set; }
        

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                ViewModel1 = new Sample5TestControlViewModel() { Value = 15 };
                ViewModel2 = new Sample5TestControlViewModel() { Value = 25 };
            }
            return base.Init();
        }

    }

    public class Sample5TestControlViewModel
    {
        public int Value { get; set; }

        public void Up()
        {
            Value++;
        }

        public void Down()
        {
            Value--;
        }
    }
}