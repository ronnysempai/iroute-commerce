/** Credenciales de inicio de sesion. */
export interface LoginRequest {
  username: string;
  password: string;
}

/** Respuesta del login con el token JWT. */
export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  expiresAt: string;
}

/** Resultado de la carga de un archivo CSV. */
export interface UploadResult {
  registrosCargados: number;
  mensaje: string;
}

/** Peticion para procesar los registros de una fecha. */
export interface ProcessRequest {
  pcProcessdate: string;
}

/** Resultado del procesamiento de una fecha. */
export interface ProcessResult {
  pcProcessdate: string;
  registrosEnCuarentena: number;
  mensaje: string;
}

/** Registro enviado a cuarentena con su motivo. */
export interface QuarantineRecord {
  id: number;
  pcId: number;
  pcNomcomred: string | null;
  pcNumdoc: string | null;
  pcTipdoc: string | null;
  pcDireccion: string | null;
  pcDepartamento: string | null;
  pcProvincia: string | null;
  pcDistrito: string | null;
  pcTelefono: string | null;
  pcEmail: string | null;
  pcProcessdate: string;
  motivo: string;
  fechaCuarentena: string;
}

/** Vista previa del CSV antes de cargarlo. */
export interface CsvPreview {
  headers: string[];
  rows: string[][];
}

/** Error devuelto por la API ({ "message": "..." }). */
export interface ApiError {
  message: string;
}
