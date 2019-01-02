using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using Monitor;
using Monitor.Interfaces;
using YamlDotNet.RepresentationModel;

public class Builder
{
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private YamlMappingNode config;
    public Builder(string configfile = "config.yaml")
    {
        using (StreamReader sr = new StreamReader(configfile))
        {
            var yaml = new YamlStream();
            yaml.Load(sr);
            config = (YamlMappingNode)yaml.Documents[0].RootNode;
        }
    }

    public Engine build()
    {
        IUrlProvider urlProvider = getProvider<IUrlProvider>("urlProvider");
        List<IValidator> validators = getProviders<IValidator>("validators");
        INotifyer notifyer = getProvider<INotifyer>("notifyer");
        Engine engine = new Engine(urlProvider, getComponetsParams("engine"));
        engine.registerValidators(validators);
        engine.registerNotifyer(notifyer);
        return engine;
    }

    private T getProvider<T>(string classification)
    {
        List<T> retProvider = getProviders<T>(classification);
        if (retProvider.Count != 1)
            log.Error($"{classification} expected to have exactly 1 data provider but got {retProvider.Count}");
        return retProvider[0];
    }


    private List<T> getProviders<T>(string classification)
    {
        List<T> retProvidersInstances = new List<T>();
        var section = (YamlMappingNode)config.Children[new YamlScalarNode("providers")];
        var providers = (YamlMappingNode)section.Children[new YamlScalarNode(classification)];
        foreach (var provider in providers)
        {
            Dictionary<string, object> compConfig = getComponetsParams(provider.Key);
            T prov = loadAssembly<T>((string)provider.Value, compConfig);
            retProvidersInstances.Add(prov);
        }
        return retProvidersInstances;
    }

    private Dictionary<string, object> getComponetsParams(YamlNode key)
    {
        var section = (YamlMappingNode)config.Children[new YamlScalarNode("components-params")];
        if (!section.Children.ContainsKey(key)) return null;
        var comsParams = (YamlMappingNode)section.Children[key];
        return getCompsParamsRec(comsParams);
    }

    Dictionary<string, object> getCompsParamsRec(YamlMappingNode node)
    {
        Dictionary<string, object> retParams = new Dictionary<string, object>();
        foreach (var child in node)
        {
            switch (child.Value.NodeType)
            {
                case YamlNodeType.Scalar:
                    retParams.Add((string)child.Key, (string)child.Value);
                    break;
                case YamlNodeType.Mapping:
                    retParams.Add((string)child.Key, getCompsParamsRec((YamlMappingNode)child.Value));
                    break;
                default:
                    log.Error("Invalid file structure, components-params node should be only of mapping nodes (key-value) types");
                    break;
            }
        }

        return retParams;
    }

    private T loadAssembly<T>(string classname, params object[] args)
    {
        Assembly asm = this.GetType().Assembly;
        Type ty = asm.GetType(classname);
        return (T)Activator.CreateInstance(ty, args);
    }
}