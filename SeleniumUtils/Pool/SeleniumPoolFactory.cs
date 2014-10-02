using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoneFin.SeleniumUtils.Pool
{
  public static class SeleniumPoolFactory
  {
    public static SeleniumPool GetPool(SeleniumPoolOptions options)
    {
      return new SeleniumPool(options);
    }
  }

}
