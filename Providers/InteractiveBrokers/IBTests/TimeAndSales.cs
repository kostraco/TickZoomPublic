#region Copyright
/*
 * Copyright 2008 M. Wayne Walter
 * Software: TickZoom Trading Platform
 * User: Wayne Walter
 * 
 * You can use and modify this software under the terms of the
 * TickZOOM General Public License Version 1.0 or (at your option)
 * any later version.
 * 
 * Businesses are restricted to 30 days of use.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * TickZOOM General Public License for more details.
 *
 * You should have received a copy of the TickZOOM General Public
 * License along with this program.  If not, see
 * <http://www.tickzoom.org/wiki/Licenses>.
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

using NUnit.Framework;
using TickZoom.Api;
using TickZoom.InteractiveBrokers;

namespace TickZoom.Test
{
	[TestFixture]
	public class TimeAndSales
	{
		private static readonly Log log = Factory.Log.GetLogger(typeof(TimeAndSales));
		private static readonly bool debug = log.IsDebugEnabled;		
		private Provider provider;
		protected SymbolInfo symbol;
		protected VerifyFeed verify;
			
		[TestFixtureSetUp]
		public virtual void Init()
		{
			string appData = Factory.Settings["AppDataFolder"];
			File.Delete( appData + @"\Logs\IBProviderTests.log");
			File.Delete( appData + @"\Logs\IBProviderService.log");
  			symbol = Factory.Symbol.LookupSymbol("CSCO");
		}
		
		[TestFixtureTearDown]
		public void Dispose()
		{
		}
		
		public void CreateProvider(bool inProcessFlag) {
			if( inProcessFlag) {
				provider = new IBInterface();
			} else {
				provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,"IBProviderService.exe");
			}
			verify = new VerifyFeed();
			provider.Start(verify);
		}
		
		[SetUp]
		public void Setup() {
			CreateProvider(true);
		}
		
		[TearDown]
		public void TearDown() {
	  		provider.Stop(verify);	
  			provider.Stop();	
		}
		
		[Test]
		public void DemoConnectionTest() {
			if(debug) log.Debug("===DemoConnectionTest===");
			if(debug) log.Debug("===StartSymbol===");
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(2,AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
		}
		
		[Test]		
		public void DemoStopSymbolTest() {
			if(debug) log.Debug("===DemoConnectionTest===");
			if(debug) log.Debug("===StartSymbol===");
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(2,AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
			if(debug) log.Debug("===StopSymbol===");
  			provider.StopSymbol(verify,symbol);
  			verify.TickQueue.Clear();
  			count = verify.Verify(0,AssertTick,symbol,10);
  			Assert.AreEqual(0,count,"tick count");
		}

		[Test]
		public void DemoReConnectionTest() {
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
  			long count = verify.Verify(2,AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
  			provider.Stop();
  			CreateProvider(true);
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
  			count = verify.Verify(2,AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
		}

		[Test]
		public void TestSeperateProcess() {
			provider.Stop();
			CreateProvider(false);
  			provider.StartSymbol(verify,symbol,TimeStamp.MinValue);
			if(debug) log.Debug("===VerifyFeed===");
  			long count = verify.Verify(2,AssertTick,symbol,25);
  			Assert.GreaterOrEqual(count,2,"tick count");
  			Process[] processes = Process.GetProcessesByName("IBProviderService");
  			Assert.AreEqual(1,processes.Length,"Number of MBTradingService processes.");
		}
		
		public virtual void AssertTick( TickIO tick, TickIO lastTick, ulong symbol) {
        	Assert.IsFalse(tick.IsQuote);
        	if( tick.IsQuote) {
	        	Assert.Greater(tick.Bid,0);
	        	Assert.Greater(tick.BidLevel(0),0);
	        	Assert.Greater(tick.Ask,0);
	        	Assert.Greater(tick.AskLevel(0),0);
        	}
        	Assert.IsTrue(tick.IsTrade);
        	if( tick.IsTrade) {
	        	Assert.Greater(tick.Price,0);
    	    	Assert.Greater(tick.Size,0);
        	}
    		Assert.IsTrue(tick.Time>=lastTick.Time,"tick.Time > lastTick.Time");
    		Assert.AreEqual(symbol,tick.lSymbol);
		}
	}
}