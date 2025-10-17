namespace RPA_ULTRA_CORE.ViewModels
{
    /// <summary>
    /// Estados da ferramenta de desenho
    /// </summary>
    public enum ToolState
    {
        Idle,           // Nenhuma ação
        DrawingLine,    // Desenhando linha (SHIFT pressionado)
        DraggingShape,  // Movendo forma
        DraggingHandle, // Movendo handle de linha
        MarqueeSelect,  // Seleção retangular
        Panning         // Movendo canvas
    }
}