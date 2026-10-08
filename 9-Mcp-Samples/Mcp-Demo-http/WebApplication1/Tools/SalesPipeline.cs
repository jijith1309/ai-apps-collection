using ModelContextProtocol.Server;
using System.ComponentModel;

public class Company
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Industry { get; set; }
    public string? Description { get; set; }
}

[McpServerToolType]
public class SalesPipelineTools
{
    [McpServerTool(Name = "GetHttpSalesPipeline")]
    [Description("Return my sales details")]
    public List<Company> GetMySalesPipeline()
    {
        // In a real implementation, this would likely query a database or an API
        return new List<Company>
            {
                new Company { Id = 1, Name = "TechCorp Solutions", Industry = "Technology", Description = "Leading cloud infrastructure provider specializing in enterprise solutions" },
                new Company { Id = 2, Name = "FinanceFirst Bank", Industry = "Financial Services", Description = "Digital banking platform with focus on customer experience" },
                new Company { Id = 3, Name = "HealthTech Innovations", Industry = "Healthcare", Description = "AI-powered healthcare analytics and patient management systems" },
                new Company { Id = 4, Name = "RetailMax Systems", Industry = "Retail", Description = "Omnichannel retail solutions for e-commerce and brick-and-mortar stores" },
                new Company { Id = 5, Name = "ManufacturePro Ltd", Industry = "Manufacturing", Description = "Industrial IoT and supply chain optimization platform" },
                new Company { Id = 6, Name = "EnergyNow Global", Industry = "Energy", Description = "Renewable energy management and smart grid technology provider" },
                new Company { Id = 7, Name = "EducationHub Platform", Industry = "Education", Description = "Online learning management system for universities and training centers" },
                new Company { Id = 8, Name = "LogisticsFlow Networks", Industry = "Logistics", Description = "Real-time tracking and route optimization for delivery services" },
                new Company { Id = 9, Name = "MediaStream Entertainment", Industry = "Media & Entertainment", Description = "Content distribution and streaming platform with original productions" },
                new Company { Id = 10, Name = "AgriBusiness Connect", Industry = "Agriculture", Description = "Farm-to-table supply chain and agricultural data analytics solution" }
            };
    }
}