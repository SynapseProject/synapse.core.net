﻿using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Synapse.Core
{
    public partial class Plan : IActionContainer, ICrypto
    {
        public Plan()
        {
            Actions = new List<ActionItem>();
            StartInfo = new PlanStartInfo();
        }

        public void EnsureInitialized()
        {
            if( Result == null )
                Result = new ExecuteResult();
        }

        public string Name { get; set; }
        public string UniqueName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public string DefaultHandlerType { get; set; }
        [YamlIgnore]
        public bool HasDefaultHandlerType { get { return !string.IsNullOrWhiteSpace( DefaultHandlerType ); } }

        public string DefaultSecurityContextProviderType { get; set; }
        [YamlIgnore]
        public bool HasDefaultSecurityContextProviderType { get { return !string.IsNullOrWhiteSpace( DefaultSecurityContextProviderType ); } }

        public string DefaultCrytoProviderType { get; set; }
        [YamlIgnore]
        public bool HasDefaultCrytoProviderType { get { return !string.IsNullOrWhiteSpace( DefaultCrytoProviderType ); } }

        ActionItem IActionContainer.ActionGroup
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public List<ActionItem> Actions { get; set; }

        public SecurityContext RunAs { get; set; }
        [YamlIgnore]
        public bool HasRunAs { get { return RunAs != null; } }

        public CryptoProvider Crypto { get; set; }
        [YamlIgnore]
        public bool HasCrypto { get { return Crypto != null; } }

        public PlanStartInfo StartInfo { get; set; }
        [YamlIgnore]
        public bool HasStartInfo { get { return StartInfo != null; } }

        public ExecuteResult Result { get; set; }
        [YamlIgnore]
        public bool HasResult { get { return Result != null; } }

        public long InstanceId { get; set; }

        [YamlIgnore]
        public Plan ResultPlan { get; set; }


        //24 Mar, 2017: not in use
        //[YamlIgnore]
        //public object _id { get; set; }
        public string LastModified { get; set; } = DateTime.Now.ToString();

        public string Signature { get; set; }


        public string ToYaml(bool emitDefaultValues = false)
        {
            return Utilities.YamlHelpers.Serialize( this, emitDefaultValues: emitDefaultValues ); ;
        }

        public void ToYaml(TextWriter tw)
        {
            Serializer serializer = new Serializer();
            serializer.Serialize( tw, this );
        }

        public static Plan FromYaml(TextReader reader)
        {
            Deserializer deserializer = new Deserializer();
            return deserializer.Deserialize<Plan>( reader );
        }

        public static Plan FromYaml(string path)
        {
            Plan plan = null;
            using( StreamReader sr = new StreamReader( path ) )
                plan = Plan.FromYaml( sr );
            return plan;
        }

        public static Dictionary<object, object> FromYamlAsDictionary(string plan)
        {
            object o = null;

            using( StringReader reader = new StringReader( plan ) )
            {
                Deserializer deserializer = new Deserializer();
                o = deserializer.Deserialize( reader );
            }

            return o as Dictionary<object, object>;
        }

        public static Plan CreateSample()
        {
            Plan p = new Plan()
            {
                Name = "SamplePlan",
                Description = "Sample Plan, all features shown.",
                UniqueName = "Human-knowable name for database lookups",
                DefaultHandlerType = "Synapse.Handlers.CommandLine:CommandHandler",
                InstanceId = 0,
                IsActive = true,
                Signature = "RSA Cryptographic signature, applied at runtime.",
                Crypto = CryptoProvider.CreateSample( addElements: false ),
                RunAs = SecurityContext.CreateSample(),
                Result = ExecuteResult.CreateSample()
            };
            p.Actions.Add( ActionItem.CreateSample() );

            return p;
        }
    }
}