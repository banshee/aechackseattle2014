﻿module ToolFinderServer

open System.IO
open System.Net

type ToolEntry =
    { toolName: string 
      toolId : string }

type RfidReads =
    { timestamp: string
      reads: RfidRead[] }

and RfidRead =
    { tag: string
      count: int
      strength: int }

let toolNames =
    Map.ofSeq([("GEN2:E2004940F54BE57110CF3F95", "Grinder")
               ("GEN2:E2004940F54BE5B110CF3F96", "Hammer drill")
               ("GEN2:E2004940F54BE43110CF3F90", "Screw gun")
               ("GEN2:AD1901011A0D019529000024", "Sawzall")
               ("GEN2:AD1901011A0CAD913000001C", "Unobtainuim smacker")
               ("GEN2:E2004940F54BE77110CF3F9D", "Nailgun")])

let expectedIds: Set<string> =
    ["GEN2:E2004940F54BE57110CF3F95"; "GEN2:AD1901011A0CAD913000001C"; "GEN2:E2004940F54BE77110CF3F9D"]
    |> Set.ofList

let AsyncHttp(url : string) = 
    let request =
        async { // Create the web request object
            let req = WebRequest.Create(url)
            // Get the response, asynchronously
            let! rsp = req.AsyncGetResponse()
            // Grab the response stream and a reader. Clean up when we're done
            use stream = rsp.GetResponseStream()
            use reader = new System.IO.StreamReader(stream)
            // synchronous read-to-end
            return reader.ReadToEnd()
        }

    request

let AsyncFile(filename: string) =
    let request =
        async { // Create the web request object
            let req = File.OpenText(filename)
            // Get the response, asynchronously
            let! rsp =  req.ReadToEndAsync() |> Async.AwaitTask
            // Grab the response stream and a reader. Clean up when we're done
            return rsp
        }

    request

let compareToolEntry a b = ()

let idToName id =
    let nameOption = toolNames |> Map.tryFind id
    let name =
        match nameOption with
        | Some n -> n
        | None -> "no name"
    name

let idToItem id =
    { ToolEntry.toolName = idToName id
      toolId = id }

let parseTags (tagsAsString: string) =
    let items = 
        Newtonsoft.Json.JsonConvert.DeserializeObject<RfidReads[]>(tagsAsString)
//        |> Array.sortBy (fun x -> x.timestamp)
        |> Array.rev
        |> Array.toList
        |> Seq.truncate 1
        |> Array.ofSeq
    let convertedItems =
        seq {
            for i in items do
                for j in i.reads do
                    let name = idToName j.tag
                    yield { ToolEntry.toolName = name; toolId = j.tag } }
    convertedItems
    |> Set.ofSeq
//    |> Seq.sort
    |> Seq.toArray

let actualIds (data: string) =
    let actuals = parseTags data
    actuals
    |> Array.map (fun x -> x.toolId)
    |> Set.ofArray

let actualItems (data: string) =
    parseTags data
//    |> Array.sortBy (fun x -> x.toolName.ToUpper())

let missingItems (data: string) =
    let actuals = actualIds data
    let missing = Set.difference expectedIds actuals
    missing
    |> Set.map idToItem
    |> Set.toArray
//    |> Array.sortBy (fun x -> x.toolName.ToUpper())

let expectedItems (data: string): ToolEntry[] =
    expectedIds
    |> Set.map idToItem
    |> Set.toArray
//    |> Array.sortBy (fun x -> x.toolName.ToUpper())

let getData() =
    let filename = "reads.json"
    async {
        let! str = AsyncHttp "http://192.168.1.112/reads.json"
        return str
//        with 
//        | e -> 
//            printfn "got exception %A" e
//            """[
//{ "timestamp": 1410645102688, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"4","strength":"-41"},{"tag":"GEN2:AD1901011A0D019529000024","count":"3","strength":"-30"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]}
//]""
//
    } 
    |> Async.StartAsTask
