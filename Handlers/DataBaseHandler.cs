using LiteDB;
using LittleWeebLibrary.StaticClasses;
using Newtonsoft.Json.Linq;
using PCLExt.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LittleWeebLibrary.Handlers
{

    public interface IDataBaseHandler
    {
        Task<JArray> GetCollection(string collection);
        Task<JObject> GetJObject(string collection, string id);
        Task<JObject> GetJObject(string collection, string property, string value);
        Task<bool> StoreJObject(string collection, JObject toStore, string id = "");
        Task<bool> UpdateJObject(string collection, JObject toUpdate, string property, string value);
        Task<bool> UpdateJObject(string collection, JObject toUpdate, string id = "");
        Task<bool> RemoveJObject(string collection, string property, string value);
        Task<bool> RemoveJObject(string collection, string id = "");
    }
    public class DataBaseHandler : IDataBaseHandler
    {
        private readonly IDebugHandler DebugHandler;
        private readonly string DataBasePath = "";

        public DataBaseHandler(IDebugHandler debugHandler)
        {
            debugHandler.TraceMessage("Constructor Called.", DebugSource.CONSTRUCTOR, DebugType.ENTRY_EXIT);
            DebugHandler = debugHandler;

            DataBasePath = PortablePath.Combine(UtilityMethods.BasePath(), "DataBase");

            try
            {
                if (!Directory.Exists(DataBasePath))
                {
                    Directory.CreateDirectory(DataBasePath);
                }
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("FAILED OPENING OR CREATING DATABASE!", DebugSource.CONSTRUCTOR, DebugType.ERROR);
                DebugHandler.TraceMessage(e.ToString(), DebugSource.CONSTRUCTOR, DebugType.ERROR);
            }
        }

        public async Task<JArray> GetCollection(string collection)
        {
            DebugHandler.TraceMessage("GetCollection called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);

            string collectionPath = PortablePath.Combine(DataBasePath, collection);
            DebugHandler.TraceMessage("Collection Path: " + collectionPath, DebugSource.TASK, DebugType.INFO);

            if (!Directory.Exists(collectionPath)) {
                Directory.CreateDirectory(collectionPath);          
            }

            string[] jsonFiles = Directory.GetFiles(collectionPath);

            JArray collectionlist = new JArray();

            foreach (string path in jsonFiles)
            {
                if (Path.GetExtension(path) == ".jzip") {
                    byte[] data = await UtilityMethods.ReadBinaryFile(path);
                    string content = await UtilityMethods.Unzip(data);
                    JObject jObject = JObject.Parse(content);
                    collectionlist.Add(jObject);
                }
            }

            return collectionlist;            
        }

        public async Task<JObject> GetJObject(string collection, string id)
        {

            DebugHandler.TraceMessage("GetJObject called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Id: " + id, DebugSource.TASK, DebugType.PARAMETERS);

            JObject toReturn = new JObject();


            string collectionPath = PortablePath.Combine(DataBasePath, collection);
            DebugHandler.TraceMessage("Collection Path: " + collectionPath, DebugSource.TASK, DebugType.INFO);
            string path = PortablePath.Combine(collectionPath, id + ".jzip");
            DebugHandler.TraceMessage("Collection Path + File: " + path, DebugSource.TASK, DebugType.INFO);
            try
            {
                if (!Directory.Exists(collectionPath))
                {
                    Directory.CreateDirectory(collectionPath);
                }
                else
                {

                    if (File.Exists(path))
                    {
                        byte[] data = await UtilityMethods.ReadBinaryFile(path);
                        string content = await UtilityMethods.Unzip(data);
                        JObject jObject = JObject.Parse(content);
                        toReturn = jObject;
                    }
                    else
                    {
                        DebugHandler.TraceMessage("Failed to open file: " + path + ", file not found!", DebugSource.TASK, DebugType.ERROR);
                    }                   
                    
                }

                
            }
            catch (IOException ioe)
            {
                DebugHandler.TraceMessage("Failed to open file: " + path + ", error: " + ioe.ToString(), DebugSource.TASK, DebugType.ERROR);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Failed to open file: " + path + ", error: " + e.ToString(), DebugSource.TASK, DebugType.ERROR);
            }


            

            return toReturn;
        }

        public async Task<JObject> GetJObject(string collection, string property, string value)
        {

            DebugHandler.TraceMessage("GetJObject called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Property: " + property, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Value: " + value, DebugSource.TASK, DebugType.PARAMETERS);

            JObject toReturn = new JObject();

            string collectionPath = PortablePath.Combine(DataBasePath, collection);
            DebugHandler.TraceMessage("Collection Path: " + collectionPath, DebugSource.TASK, DebugType.INFO);
            string pathparsing = "";
            try
            {
                if (!Directory.Exists(collectionPath))
                {
                    Directory.CreateDirectory(collectionPath);
                }

                string[] jsonFiles = Directory.GetFiles(collectionPath);

                foreach (string path in jsonFiles)
                {
                    pathparsing = path;
                    if (Path.GetExtension(path) == ".jzip")
                    {
                        byte[] data = await UtilityMethods.ReadBinaryFile(path);
                        string content = await UtilityMethods.Unzip(data);
                        JObject jObject = JObject.Parse(content);

                        if (jObject.Value<string>(property) == value)
                        {
                            toReturn = jObject;
                            break;
                        }
                    }
                }
            }
            catch (IOException ioe)
            {
                DebugHandler.TraceMessage("Failed to open file: " + pathparsing + ", error: " + ioe.ToString(), DebugSource.TASK, DebugType.ERROR);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Failed to open file: " + pathparsing + ", error: " + e.ToString(), DebugSource.TASK, DebugType.ERROR);
            }


            

            return toReturn;
        }

        public async Task<bool> RemoveJObject(string collection, string id)
        {

            DebugHandler.TraceMessage("RemoveJObject called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("ID: " + id, DebugSource.TASK, DebugType.PARAMETERS);

            string collectionPath = PortablePath.Combine(DataBasePath, collection);
            DebugHandler.TraceMessage("Collection Path: " + collectionPath, DebugSource.TASK, DebugType.INFO);

            bool toReturn = false;

            await Task.Run(() =>
            {
                if (Directory.Exists(collectionPath))
                {

                    string toRemove = PortablePath.Combine(collectionPath, id + ".jzip");
                    try
                    {

                        if (File.Exists(toRemove))
                        {
                            File.Delete(toRemove);
                            toReturn = true;
                        }
                        else
                        {
                            DebugHandler.TraceMessage("Failed to remove file: " + toRemove + ", error: File not found!", DebugSource.TASK, DebugType.ERROR);
                        }
                    }
                    catch (IOException ioe)
                    {
                        DebugHandler.TraceMessage("Failed to remove file: " + toRemove + ", error: " + ioe.ToString(), DebugSource.TASK, DebugType.ERROR);
                    }
                    catch (Exception e)
                    {
                        DebugHandler.TraceMessage("Failed to remove file: " + toRemove + ", error: " + e.ToString(), DebugSource.TASK, DebugType.ERROR);
                    }
                }
            });
           
            

            return toReturn;
        }

        public async Task<bool> RemoveJObject(string collection, string property, string value)
        {

            DebugHandler.TraceMessage("RemoveJObject called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Property: " + property, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Value: " + value, DebugSource.TASK, DebugType.PARAMETERS);

            string collectionPath = PortablePath.Combine(DataBasePath, collection);
            DebugHandler.TraceMessage("Collection Path: " + collectionPath, DebugSource.TASK, DebugType.INFO);

            bool toReturn = false;

            if (Directory.Exists(collectionPath))
            {
               
                string toRemove = "";
                try
                {
                    string[] jsonFiles = Directory.GetFiles(collectionPath);

                    foreach (string path in jsonFiles)
                    {
                        if (Path.GetExtension(path) == ".jzip")
                        {
                            byte[] data = await UtilityMethods.ReadBinaryFile(path);
                            string content = await UtilityMethods.Unzip(data);
                            JObject jObject = JObject.Parse(content);

                            if (jObject.Value<string>(property) == value)
                            {
                                toReturn = true;
                                toRemove = path;
                                break;
                            }
                        }
                    }
                    File.Delete(toRemove);
                }
                catch (IOException ioe)
                {
                    DebugHandler.TraceMessage("Failed to remove file: " + toRemove + ", error: " + ioe.ToString(), DebugSource.TASK, DebugType.ERROR);
                }
                catch (Exception e)
                {
                    DebugHandler.TraceMessage("Failed to remove file: " + toRemove + ", error: " + e.ToString(), DebugSource.TASK, DebugType.ERROR);
                }
            }

            return toReturn;
        }

        public async Task<bool> StoreJObject(string collection, JObject toStore, string id = "")
        {

            DebugHandler.TraceMessage("StoreJObject called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);


             bool toReturn = false;

            string collectionPath = PortablePath.Combine(DataBasePath, collection);

            if (id == string.Empty)
            {
                id = Guid.NewGuid().ToString("N");
            }

            DebugHandler.TraceMessage("Collection Path: " + collectionPath, DebugSource.TASK, DebugType.INFO);
            string pathparsing = PortablePath.Combine(collectionPath, id + ".jzip");
            try
            {
                if (!Directory.Exists(collectionPath))
                {
                    Directory.CreateDirectory(collectionPath);
                }

                toStore["_id"] = id;

                byte[] zipped = await UtilityMethods.Zip(toStore.ToString());
                await UtilityMethods.WriteBinaryFile(pathparsing, zipped);

                toReturn = true;
            }
            catch (IOException ioe)
            {
                DebugHandler.TraceMessage("Failed to write file: " + pathparsing + ", error: " + ioe.ToString(), DebugSource.TASK, DebugType.ERROR);
            }
            catch (Exception e)
            {
                DebugHandler.TraceMessage("Failed to write file: " + pathparsing + ", error: " + e.ToString(), DebugSource.TASK, DebugType.ERROR);
            }          
            

            return toReturn;
        }

        public async Task<bool> UpdateJObject(string collection, JObject toUpdate, string id)
        {
            DebugHandler.TraceMessage("UpdateJObject called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Id: " + id, DebugSource.TASK, DebugType.PARAMETERS);

            bool toReturn = false;

            JObject old = await GetJObject(collection, id);

            if (old.Count > 0)
            {
                JsonMergeSettings mergeSettings = new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                };
                old.Merge(toUpdate, mergeSettings);

                toReturn = await StoreJObject(collection, old, id);
            }
            else
            {
                toReturn = await StoreJObject(collection, toUpdate, id);
            }

            return toReturn;
        }

        public async Task<bool> UpdateJObject(string collection, JObject toUpdate, string property, string value)
        {
            DebugHandler.TraceMessage("UpdateJObject called.", DebugSource.TASK, DebugType.ENTRY_EXIT);
            DebugHandler.TraceMessage("Collection: " + collection, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Property: " + property, DebugSource.TASK, DebugType.PARAMETERS);
            DebugHandler.TraceMessage("Value: " + value, DebugSource.TASK, DebugType.PARAMETERS);

            bool toReturn = false;

            JObject old = await GetJObject(collection, property, value);

            if (old.Count > 0)
            {
                JsonMergeSettings mergeSettings = new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                };
                old.Merge(toUpdate, mergeSettings);
                toReturn = await StoreJObject(collection, old, old.Value<string>("_id"));
            }
            else
            {
                toReturn = await StoreJObject(collection, toUpdate);
            }

            return toReturn;
        }
    }
}
