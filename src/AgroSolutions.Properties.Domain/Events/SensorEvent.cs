using AgroSolutions.Properties.Domain.Enums;

namespace AgroSolutions.Properties.Domain.Events;

/// <summary>
/// Evento unificado de sensor publicado no tópico agrosolutions-property-events.
/// Contrato alinhado ao Worker/Ingestion service.
/// </summary>
public class SensorEvent
{
    /// <summary>ID do Talhão ao qual o sensor pertence</summary>
    public Guid FieldId { get; set; }

    /// <summary>ID do Sensor</summary>
    public Guid SensorId { get; set; }

    /// <summary>Data de criação/instalação do sensor</summary>
    public DateTime DtCreated { get; set; }

    /// <summary>Categoria do sensor: Solo=1, Silos=2, Meteorologica=3</summary>
    public TypeSensor TypeSensor { get; set; }

    /// <summary>true = ativo, false = inativo</summary>
    public bool StatusSensor { get; set; }

    /// <summary>Tipo de operação: Create=1, Update=2, Delete=3</summary>
    public TypeOperation TypeOperation { get; set; }

    /// <summary>
    /// Mapeia TipoSensor (modelo de domínio) para TypeSensor (contrato do evento).
    /// UmidadeSolo / pH → Solo
    /// Temperatura / Precipitacao / UmidadeAr / Luminosidade / VelocidadeVento → Meteorologica
    /// </summary>
    public static TypeSensor MapTipoSensor(TipoSensor tipo) =>
        tipo switch
        {
            TipoSensor.UmidadeSolo => TypeSensor.Solo,
            TipoSensor.pH => TypeSensor.Solo,
            TipoSensor.Temperatura => TypeSensor.Meteorologica,
            TipoSensor.Precipitacao => TypeSensor.Meteorologica,
            TipoSensor.UmidadeAr => TypeSensor.Meteorologica,
            TipoSensor.Luminosidade => TypeSensor.Meteorologica,
            TipoSensor.VelocidadeVento => TypeSensor.Meteorologica,
            _ => TypeSensor.Meteorologica,
        };
}
