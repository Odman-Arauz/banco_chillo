import { z } from 'zod';

// Validación de entrada (equivalente a FluentValidation en .NET).

export const crearPlantillaEsquema = z.object({
  codigo: z
    .string()
    .trim()
    .regex(/^[A-Z_]{3,50}$/, 'El código debe estar en MAYÚSCULAS_CON_GUION_BAJO (3 a 50 caracteres).'),
  canal: z.enum(['EMAIL', 'SMS', 'PUSH'], { message: 'El canal debe ser EMAIL, SMS o PUSH.' }),
  asunto: z.string().trim().min(3).max(150),
  cuerpo: z.string().trim().min(5).max(2000),
});

export const actualizarPlantillaEsquema = crearPlantillaEsquema.omit({ codigo: true }).extend({
  activa: z.boolean().optional(),
});

export const idEsquema = z.coerce.number().int().positive();
