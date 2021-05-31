using System;
using System.Windows.Controls;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using Prism.Regions.Behaviors;

namespace Issue
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Initialize infrastructure
            ContainerLocator.SetContainerExtension(() => new DryIocContainerExtension(new Container(DryIocContainerExtension.DefaultRules)));
            var container = ContainerLocator.Current;
            var regionBehaviorFactory = new RegionBehaviorFactory(container);
            var regionManager = new RegionManager();

            var regionAdapterMappings = container.Resolve<RegionAdapterMappings>();
            IRegionAdapter adapter = new ContentControlRegionAdapter(regionBehaviorFactory);
            regionAdapterMappings.RegisterMapping(typeof(ContentControl), adapter);
            container.Register<DelayedRegionCreationBehavior>(() => new DelayedRegionCreationBehavior(regionAdapterMappings));

            // Initialize controls            
            var childControl = new ContentControl();
            var child = new UserControl() { Content = childControl };
            
            var parentControl = new ContentControl();
            var parent = new UserControl() { Content = parentControl };

            // Create parent region
            SingleActiveRegion parentRegion = (SingleActiveRegion)adapter.Initialize(parentControl, "Region1");            
            var behavior = new RegionManagerRegistrationBehavior()
            {
                HostControl = parentControl,
                Region = parentRegion
            };
            behavior.Attach();
            RegionManager.SetRegionManager(parent, regionManager);

            // Create child region
            SingleActiveRegion childRegion = (SingleActiveRegion)adapter.Initialize(childControl, "Region2");
            var behavior1 = new RegionManagerRegistrationBehavior()
            {
                HostControl = childControl,
                Region = childRegion
            };
            behavior1.Attach();
            RegionManager.SetRegionManager(child, regionManager);

            // Add control to region
            regionManager.AddToRegion("Region1", child);
            RegionManager.UpdateRegions();

            // Remove it
            parentRegion.Remove(child);

            // Create scoped region
            var scopedRegion = regionManager.CreateRegionManager();
            RegionManager.SetRegionManager(parent, scopedRegion);
            scopedRegion.AddToRegion("Region1", child);

            RegionManager.UpdateRegions();
        }
    }
}
