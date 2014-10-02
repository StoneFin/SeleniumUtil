using Nancy;
using Nancy.Conventions;
using Nancy.TinyIoc;

namespace IntegrationTestViewer
{
  public class Bootstrapper : DefaultNancyBootstrapper
  {
    // The bootstrapper enables you to reconfigure the composition of the framework,
    // by overriding the various methods and properties.
    // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

    // Need to override this, not just make a new method
    protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
    {
      //register the repository booyah
      container.Register<IntegrationTestViewer.Models.IRepository, IntegrationTestViewer.Models.TestRepository>();
    }
    protected override void ConfigureConventions(NancyConventions nancyConventions)
    {
      nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("Scripts", @"Scripts"));
      nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("fonts", @"fonts"));
      base.ConfigureConventions(nancyConventions);
    }

  }
}