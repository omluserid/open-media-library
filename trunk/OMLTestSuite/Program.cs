﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OMLTestSuite
{
    class Program
    {
        static void Main(string[] args)
        {
            MyMoviesPluginTest mmpt = new MyMoviesPluginTest();
            mmpt.TEST_BASE_CASE();

            TitleTest tt = new TitleTest();
            tt.TEST_BASE_CASE();

            TitleCollectionTest tct = new TitleCollectionTest();
            tct.TEST_BASE_CASE();
            tct.TEST_FIND_FOR_ID();
            tct.TEST_SOURCE_DATABASE_TO_USE();
            tct.TEST_TO_DATASET_TABLE_OBJECT();
            tct.TEST_REPLACE_METHOD();

            OMLConfigManagerTest omlcm = new OMLConfigManagerTest();
            omlcm.TEST_BASE_CASE();
        }
    }
}
