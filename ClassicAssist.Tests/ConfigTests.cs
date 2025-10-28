﻿using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClassicAssist.Tests
{
    [TestClass]
    [Serializable]
    public class ConfigTests
    {
        private string _profilePath;

        [TestMethod]
        public void WontThrowExceptionOnDeserializeNullConfig()
        {
            TestConfig( null );
        }

        [TestMethod]
        public void WontThrowExceptionOnDeserializeEmptyConfig()
        {
            TestConfig( new JObject() );
        }

        public void TestConfig( JObject json )
        {
            bool setNull = json == null;

            Engine.StartupPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            _profilePath = Path.Combine( Engine.StartupPath, "Profiles" );

            if ( File.Exists( Path.Combine( _profilePath, "settings.json" ) ) )
            {
                File.Delete( Path.Combine( _profilePath, "settings.json" ) );
            }

            IEnumerable<Type> allSettingProvider = Assembly.GetAssembly( typeof( Engine ) ).GetTypes().Where( t =>
                typeof( ISettingProvider ).IsAssignableFrom( t ) && t.IsClass && !t.IsAbstract );

            Options options = new Options();

            foreach ( Type type in allSettingProvider )
            {
                if ( !type.IsPublic )
                {
                    continue;
                }

                ISettingProvider p = (ISettingProvider) Activator.CreateInstance( type );

                p.Deserialize( json, options );

                json = setNull ? null : new JObject();

                p.Serialize( json );
            }

            if ( File.Exists( Path.Combine( _profilePath, "settings.json" ) ) )
            {
                File.Delete( Path.Combine( _profilePath, "settings.json" ) );
            }
        }
    }
}