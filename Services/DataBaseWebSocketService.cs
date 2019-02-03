using LittleWeebLibrary.EventArguments;
using LittleWeebLibrary.GlobalInterfaces;
using LittleWeebLibrary.StaticClasses;
using LittleWeebLibrary.Handlers;
using LittleWeebLibrary.Models;
using LittleWeebLibrary.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Services
{
    public interface IDataBaseWebSocketService
    {
        Task GetDocument(JObject query);
        Task StoreDocument(JObject query);
        Task UpdateDocument(JObject query);
        Task DeleteDocument(JObject query);
        Task GetCollection(JObject query);
    }

    public class DataBaseWebSocketService : IDataBaseWebSocketService
    {

        private IDataBaseHandler DataBaseHandler;
        private IWebSocketHandler WebSocketHandler;
        private IDebugHandler DebugHandler;

        public DataBaseWebSocketService(IWebSocketHandler webSocketHandler, IDataBaseHandler dataBaseHandler, IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);

            DataBaseHandler = dataBaseHandler;
            WebSocketHandler = webSocketHandler;
            DebugHandler = debugHandler;
        }

        public async Task DeleteDocument(JObject query)
        {

            DebugHandler.TraceMessage("DeleteDocument called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            string collection = query.Value<string>("collection");
            bool succes = false;
            if (query.ContainsKey("parameter") && query.ContainsKey("value"))
            {
                succes = await DataBaseHandler.RemoveJObject(collection, query.Value<string>("parameter"), query.Value<string>("value"));
            }
            else if (query.ContainsKey("id"))
            {
                succes = await DataBaseHandler.RemoveJObject(collection, query.Value<string>("id"));
            }
            else
            {
                JsonError jsonError = new JsonError()
                {
                    type = "delete_document_error",
                    errormessage = "Either parameter & value or id are not defined!",
                    errortype = "warning"
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }

            if (succes)
            {
                JsonSuccess jsonSuccess = new JsonSuccess()
                {
                    message = "Succesfully deleted document!"
                };

                await WebSocketHandler.SendMessage(jsonSuccess.ToJson());
            }
            else
            {
                JsonError jsonError = new JsonError()
                {
                    type = "delete_document_error",
                    errormessage = "Failed to delete document.",
                    errortype = "warning"
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }
        }

        public async Task GetCollection(JObject query)
        {
            DebugHandler.TraceMessage("GetCollection called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            string collection = query.Value<string>("collection");
            JArray result = await DataBaseHandler.GetCollection(collection);

            if (result.Count > 0)
            {
                JsonDataBaseCollection collectionresult = new JsonDataBaseCollection()
                {
                    result = result
                };

                await WebSocketHandler.SendMessage(collectionresult.ToJson());

            }
            else
            {
                JsonError jsonError = new JsonError()
                {
                    type = "get_collection_error",
                    errormessage = "Collection is empty.",
                    errortype = "warning"
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }

        }

        public async Task GetDocument(JObject query)
        {
            DebugHandler.TraceMessage("GetDocument called.", DebugSource.TASK, DebugType.ENTRY_EXIT);

            string collection = query.Value<string>("collection");
            JObject document = new JObject();
            if (query.ContainsKey("parameter") && query.ContainsKey("value"))
            {
                document = await DataBaseHandler.GetJObject(collection, query.Value<string>("parameter"), query.Value<string>("value"));
            }
            else if (query.ContainsKey("id"))
            {
                document = await DataBaseHandler.GetJObject(collection, query.Value<string>("id"));
            }
            else
            {
                JsonError jsonError = new JsonError()
                {
                    type = "get_document_error",
                    errormessage = "Either parameter & value or id are not defined!",
                    errortype = "warning"
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }

            if (document.Count > 0)
            {
                JsonDataBaseDocument tosend = new JsonDataBaseDocument()
                {
                    result = document
                };
                await WebSocketHandler.SendMessage(tosend.ToString());
            }
            else
            {
                JsonError jsonError = new JsonError()
                {
                    type = "get_document_error",
                    errormessage = "Failed to get document.",
                    errortype = "warning"
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }
        }

        public async Task StoreDocument(JObject query)
        {
            DebugHandler.TraceMessage("StoreDocument called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            string collection = query.Value<string>("collection");
            JObject document = query.Value<JObject>("document");
            bool succes = false;
            if (query.ContainsKey("id"))
            {
                succes = await DataBaseHandler.StoreJObject(collection, document, query.Value<string>("id"));
            }
            else
            {
                succes = await DataBaseHandler.StoreJObject(collection, document);
            }

            if (succes)
            {
                JsonSuccess jsonSuccess = new JsonSuccess()
                {
                    message = "Succesfully stored document!"
                };

                await WebSocketHandler.SendMessage(jsonSuccess.ToJson());
            }
            else
            {
                JsonError jsonError = new JsonError()
                {
                    type = "store_document_error",
                    errormessage = "Failed to store document.",
                    errortype = "warning"
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }
        }

        public async Task UpdateDocument(JObject query)
        {
            DebugHandler.TraceMessage("UpdateDocument called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            string collection = query.Value<string>("collection");
            JObject document = query.Value<JObject>("document");
            bool succes = false;
            if (query.ContainsKey("id"))
            {
                succes = await DataBaseHandler.UpdateJObject(collection, document, query.Value<string>("id"));
            }
            else
            {
                succes = await DataBaseHandler.UpdateJObject(collection, document);
            }

            if (succes)
            {
                JsonSuccess jsonSuccess = new JsonSuccess()
                {
                    message = "Succesfully updated document!"
                };

                await WebSocketHandler.SendMessage(jsonSuccess.ToJson());
            }
            else
            {
                JsonError jsonError = new JsonError()
                {
                    type = "update_document_error",
                    errormessage = "Failed to update document.",
                    errortype = "warning"
                };

                await WebSocketHandler.SendMessage(jsonError.ToJson());
            }
        }
    }
}
