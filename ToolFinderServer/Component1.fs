module ToolFinderServer

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
    Map.ofSeq([("GEN2:E2004940F54BE57110CF3F95", "Warp drive wrench"); 
               ("GEN2:AD1901011A0CAD913000001C", "Unobtainuim smacker")])

let expectedTools =
    ["GEN2:E2004940F54BE57110CF3F95"; "GEN2:AD1901011A0CAD913000001C"]
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
    |> Async.StartAsTask

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

let parseTags (tagsAsString: string) =
    let items = 
        Newtonsoft.Json.JsonConvert.DeserializeObject<RfidReads[]>(tagsAsString)
        |> Array.sortBy (fun x -> x.timestamp)
        |> Array.toList
        |> Seq.truncate 1
        |> Array.ofSeq
    let convertedItems =
        seq {
            for i in items do
                for j in i.reads do
                    let nameOption = toolNames |> Map.tryFind j.tag
                    let name =
                        match nameOption with
                        | Some n -> n
                        | None -> "no name"
                    yield { ToolEntry.toolName = name; toolId = name } }
    convertedItems
    |> Set.ofSeq
    |> Seq.sort
    |> Seq.toArray

let getData(filename: string) =
    let s = """
[
{ "timestamp": 1410645102688, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"4","strength":"-41"},{"tag":"GEN2:AD1901011A0D019529000024","count":"3","strength":"-30"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645103015, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"6","strength":"-45"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-21"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645103305, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-39"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"3","strength":"-39"},{"tag":"GEN2:AD1901011A0D019529000024","count":"4","strength":"-32"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645103600, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"3","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"4","strength":"-41"},{"tag":"GEN2:AD1901011A0D019529000024","count":"4","strength":"-30"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"5","strength":"-25"},]},
{ "timestamp": 1410645103907, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-48"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-23"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"5","strength":"-27"},]},
{ "timestamp": 1410645104203, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"3","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"3","strength":"-39"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"4","strength":"-39"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645104504, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"3","strength":"-39"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"5","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"3","strength":"-39"},{"tag":"GEN2:AD1901011A0D019529000024","count":"3","strength":"-32"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"5","strength":"-25"},]},
{ "timestamp": 1410645104812, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-21"},{"tag":"GEN2:AD1901011A0D019529000024","count":"3","strength":"-30"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"5","strength":"-25"},]},
{ "timestamp": 1410645105115, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"5","strength":"-23"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645105427, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-45"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-23"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-27"},]},
{ "timestamp": 1410645105728, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"3","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"5","strength":"-21"},{"tag":"GEN2:AD1901011A0D019529000024","count":"4","strength":"-30"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645106014, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-39"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"3","strength":"-39"},{"tag":"GEN2:AD1901011A0D019529000024","count":"4","strength":"-32"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645106304, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"3","strength":"-41"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"3","strength":"-41"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"5","strength":"-25"},]},
{ "timestamp": 1410645106587, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"3","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"5","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"4","strength":"-41"},{"tag":"GEN2:AD1901011A0D019529000024","count":"4","strength":"-32"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645106874, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-21"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645107151, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"4","strength":"-45"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-21"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"5","strength":"-25"},]},
{ "timestamp": 1410645107451, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"5","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"5","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-21"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645107736, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-27"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"5","strength":"-45"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"5","strength":"-23"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
{ "timestamp": 1410645108033, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D","count":"3","strength":"-43"},{"tag":"GEN2:AD1901011A0CAD913000001C","count":"4","strength":"-18"},{"tag":"GEN2:E2004940F54BE43110CF3F90","count":"4","strength":"-41"},{"tag":"GEN2:AD1901011A0D019529000024","count":"4","strength":"-30"},{"tag":"GEN2:E2004940F54BE57110CF3F95","count":"4","strength":"-25"},]},
]
"""
    let d = Newtonsoft.Json.JsonConvert.DeserializeObject<RfidReads[]>(s)
    printfn "d is %A" d
    async {
        let! str = AsyncFile filename
        printfn "gotstr %A" str
        let data = parseTags str
        printfn "data is %A" data
        return data
    } 
    |> Async.StartAsTask

// [{ "timestamp": 1410645104504, "reads": [{"tag":"GEN2:E2004940F54BE5B110CF3F96","count":"4","strength":"-25"},{"tag":"GEN2:E2004940F54BE77110CF3F9D"]}]
