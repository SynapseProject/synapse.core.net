﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Synapse.Core;
using Synapse.Service.Windows;

namespace Synapse.Service.cli
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpApiClient winClient = new HttpApiClient( "http://localhost:8000/syn/server/" );

            string __root = @"C:\Devo\synapse\synapse.core.net\Synapse.UnitTests";
            string __plansRoot = $@"{__root}\Plans";
            string plan0Name = "planScheduler.yaml";
            string plan1Name = "planScheduler.yaml";
            string plan2Name = "planScheduler.yaml";
            string plan3Name = "planScheduler.yaml";

            Plan plan00 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
            Plan plan01 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
            Plan plan02 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
            Plan plan03 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );
            Plan plan04 = Plan.FromYaml( $"{__plansRoot}\\{plan0Name}" );
            Plan plan05 = Plan.FromYaml( $"{__plansRoot}\\{plan1Name}" );
            Plan plan06 = Plan.FromYaml( $"{__plansRoot}\\{plan2Name}" );
            Plan plan07 = Plan.FromYaml( $"{__plansRoot}\\{plan3Name}" );

            List<Plan> plans = new List<Plan>();
            plans.Add( plan00 );
            plans.Add( plan01 );
            plans.Add( plan02 );
            plans.Add( plan03 );
            plans.Add( plan04 );
            plans.Add( plan05 );
            plans.Add( plan06 );
            plans.Add( plan07 );


            int instanceId = 0;

            Parallel.For( 0, 100, ctr =>
            {
                winClient.StartPlan( instanceId++, false, plan00 );
            } );

            //Parallel.ForEach( plans, plan =>
            //{
            //    winClient.StartPlan( instanceId++, false, plan );
            //} );
        }
    }
}