using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace SeleniumDocker
{
    public class DockerController
    {

        private DockerClient client;

        public DockerController(string uri)
        {
            try
            {
                AnonymousCredentials creds = new AnonymousCredentials();

                client = new DockerClientConfiguration(
                 new Uri(uri))
                .CreateClient();
            }
            catch (Exception ex)
            { }
        }


        public async void StartContainerHub(string ImageName)
        {
            var CreateParameters = new CreateContainerParameters
            {
                Image = ImageName,
                Name = "selenium-hub",
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {
                    "4444", default(EmptyStruct)
                    }
                },

                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {"4444", new List<PortBinding> {new PortBinding {HostPort = "4444"}}}
            },
                    PublishAllPorts = true
                },
                ArgsEscaped = true,
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true,

            };

            try
            {
                CreateContainerResponse response = await client.Containers.CreateContainerAsync(CreateParameters);
                await client.Containers.StartContainerAsync(response.ID, null);
            }
            catch (Exception ex) { }

        }


        public async Task DisposeContainerAsync(string _containerId)
        {
            if (_containerId == null)
                return;

            try
            {
                await client.Containers.StopContainerAsync(_containerId,
                new ContainerStopParameters { WaitBeforeKillSeconds=2});
                
                await client.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters());
            }
            catch (Exception ex)
            { }
        }

        public async Task<string> StartContainerNode(string ImageName, string NodeName, string DriverPort, string VNCPort)
        {
            var CreateParameters = new CreateContainerParameters
            {
                Image = ImageName,
                Name = NodeName,
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {
                    "4444", default(EmptyStruct)
                    },
                     {
                    "5900", default(EmptyStruct)
                    }
                },

                HostConfig = new HostConfig
                {
                    ShmSize = 134217728,
                    PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {"4444", new List<PortBinding> {new PortBinding {HostPort = DriverPort } }},
                {"5900", new List<PortBinding> {new PortBinding {HostPort = VNCPort } }},
                
            },

                },
                ArgsEscaped = true,
                AttachStderr = true,
                AttachStdin = true,
                AttachStdout = true,

            };

            CreateContainerResponse response = await client.Containers.CreateContainerAsync(CreateParameters);

            try
            {
                Task t = client.Containers.StartContainerAsync(response.ID, null);
                t.Wait();
                Console.WriteLine($"{NodeName} container created: {DateTime.Now}");

            }
            catch (Exception ex) 
            {
                Console.WriteLine($"{NodeName} {ex.Message}");
            }
           
            return response.ID;



        }

        public void FindContainer(string ContainerName)
        {

            var containers = client.Containers.ListContainersAsync(
             new ContainersListParameters()
             {
                 All = true

             }).Result;


            foreach (var item in containers)
            {
                Console.WriteLine("Id: {0}", item.ID);
                Console.WriteLine("Names: {0}", string.Join(", ", item.Names));
                Console.WriteLine("Command: {0}", item.Command);
                Console.WriteLine("Created: {0}", item.Created);
                Console.WriteLine("Image: {0}", item.Image);
                Console.WriteLine("SizeRootFs: {0}", item.SizeRootFs);
                Console.WriteLine("SizeRw: {0}", item.SizeRw);
                Console.WriteLine("Status: {0}", item.Status);
            }
        }

    }
}
