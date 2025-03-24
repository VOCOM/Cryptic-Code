module CoreClient

open System
open System.IO
open System.Threading.Tasks
open System.Reflection

open Discord
open Discord.WebSocket

let StartClient(): DiscordSocketClient =
  let env = "Cryptic_Code..env"
  let assembly = Assembly.GetExecutingAssembly()
  let aStream = assembly.GetManifestResourceStream(env)
  let aReader = new StreamReader(aStream)
  let aContent = aReader.ReadToEnd()
  
  let token: string =
    let index = aContent.IndexOf("discord-token")
    if index >= 0 then
      let startIndex = aContent.IndexOf("\"") + 1
      let endIndex = aContent.IndexOf("\"", startIndex)
      aContent.Substring(startIndex, endIndex - startIndex)
    else
      String.Empty
  let config: DiscordSocketConfig = 
    let config = DiscordSocketConfig()
    config.GatewayIntents <- config.GatewayIntents ||| GatewayIntents.MessageContent
    config
  let client:DiscordSocketClient = new DiscordSocketClient(config)
  
  // Callbacks
  let connectCallback(): Task = Task.Run(fun () -> printfn "Connected...")
  let readyCallback(): Task = Task.Run(fun () -> printfn "Ready...")
  let messageCallback(msg: SocketMessage): Task = 
    task{ 
      printfn "Read %s from %s" msg.Content msg.Channel.Name
    }
  
  // Attach Callbacks
  client.add_Connected(new Func<Task>(connectCallback))
  client.add_Ready(new Func<Task>(readyCallback))
  client.add_MessageReceived(new Func<SocketMessage, Task>(messageCallback))
  
  // Initialization sequence
  let tasks = [
    client.LoginAsync(TokenType.Bot,token);
    client.StartAsync()
  ]
  
  // Execute initialization
  tasks
  |> List.map Async.AwaitTask
  |> ignore

  client
