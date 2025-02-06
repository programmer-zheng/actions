using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Scalar.Dtos;

public class ModelBindTestDto
{
    public string Name { get; set; }

    [Range(1, 200)]
    [DefaultValue(100)]
    public int Age { get; set; }


    public bool IsStudent { get; set; }
}