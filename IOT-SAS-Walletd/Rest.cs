using System;
using Nancy;
using FactomSharp;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using Nancy.Responses;

namespace IOTSASWalletd
{
    public class Rest : Nancy.NancyModule
    {

        public Rest()
        {
            //Post[@"/(.*)"] = _ => {
            //    Console.WriteLine("Hello");
            //    return "";
            //};

            Get["/"] = _ =>
            {
                return new TextResponse(HttpStatusCode.OK, IOT_SAS.ECAddressClass.Public);
            };
            

            Post["/v2"] = _ =>
            {
                var id = this.Request.Body;
                var length = this.Request.Body.Length;
                var data = new byte[length];
                id.Read(data, 0, (int)length);
                var body = System.Text.Encoding.Default.GetString(data);
                if (body.Contains("compose-chain"))
                {
                    Console.WriteLine("compose-chain");
                    var request = JsonConvert.DeserializeObject<FactomSharp.FactomWalletd.API.ComposeChain.ComposeChainRequest>(body);

                    var extidStrings = new List<byte[]>();
                    foreach (var extid in request.param.Chain.firstentry.Extids)
                    {
                        extidStrings.Add(Encoding.UTF8.GetBytes(extid));
                    }
                    
                                        
                    var chain = new FactomSharp.Factomd.ComposeChain(
                         Encoding.UTF8.GetBytes(request.param.Chain.firstentry.Content),
                         IOT_SAS.ECAddressClass,
                         extidStrings.ToArray());


                    var reply = new FactomSharp.FactomWalletd.API.ComposeChain.ComposeChainResult();
                    reply.result = new FactomSharp.FactomWalletd.API.ComposeChain.ComposeChainResult.Result();
                    reply.result.commit = new FactomSharp.FactomWalletd.API.ComposeChain.ComposeChainResult.Result.Commit();
                    reply.result.commit.Params = new FactomSharp.FactomWalletd.API.ComposeChain.ComposeChainResult.Result.Commit.CommitParams();
                    reply.result.commit.Params.Message = chain.GetHexString();
                    reply.result.commit.Method = "commit-chain";
                    reply.result.commit.Jsonrpc = "2.0";;
                    reply.result.commit.Id = request.Id;

                    reply.result.reveal = new FactomSharp.FactomWalletd.API.ComposeChain.ComposeChainResult.Result.Reveal();
                    reply.result.reveal.Params = new FactomSharp.FactomWalletd.API.ComposeChain.ComposeChainResult.Result.Reveal.RevealParams();
                    reply.result.reveal.Params.Entry = chain.Entry.MarshalBinary.ToHexString();
                    reply.result.reveal.Method = "reveal-chain";
                    reply.result.reveal.Jsonrpc = "2.0";;
                    reply.result.reveal.Id = request.Id;

                    reply.Id = request.Id;
                    reply.Jsonrpc = request.Jsonrpc;


                    return new TextResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(reply));

                }
                else if (body.Contains("compose-entry"))
                {
                    var request = JsonConvert.DeserializeObject<FactomSharp.FactomWalletd.API.ComposeEntry>(body);
                    
                    var extidStrings = new List<byte[]>();
                    foreach (var extid in request.Request.param.entry.Extids)
                    {
                        extidStrings.Add(Encoding.UTF8.GetBytes(extid));
                    }
                    
                    var entry = new FactomSharp.Factomd.ComposeEntry(
                         request.Request.param.entry.Chainid,
                         Encoding.UTF8.GetBytes(request.Request.param.entry.Content),
                         IOT_SAS.ECAddressClass,
                         extidStrings.ToArray());

                    var reply = new FactomSharp.FactomWalletd.API.ComposeEntry.ComposeEntryResult();
                    reply.result = new FactomSharp.FactomWalletd.API.ComposeEntry.ComposeEntryResult.Result();
                    reply.result.commit = new FactomSharp.FactomWalletd.API.ComposeEntry.ComposeEntryResult.Result.Commit();
                    reply.result.commit.Params = new FactomSharp.FactomWalletd.API.ComposeEntry.ComposeEntryResult.Result.CommitParams();
                    reply.result.commit.Params.Message = entry.GetHexString();
                    reply.result.commit.Id = request.Request.Id;
                    reply.result.commit.Method = "commit-entry";
                    reply.result.commit.Jsonrpc = "2.0";


                    reply.result.reveal = new FactomSharp.FactomWalletd.API.ComposeEntry.ComposeEntryResult.Result.Reveal();
                    reply.result.reveal.Params = new FactomSharp.FactomWalletd.API.ComposeEntry.ComposeEntryResult.Result.RevealParams();
                    reply.result.reveal.Params.Entry = entry.DataEntry.ToHexString();
                    reply.result.reveal.Id = request.Request.Id;
                    reply.result.reveal.Method = "reveal-entry";
                    reply.result.reveal.Jsonrpc = "2.0";


                    return new TextResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(reply));

                }
                else
                {
                    return 200;
                }
            };
        }
    }
}