export interface UserViewModel {
  id: number;
  nome: string;
  email: string;
  role: string;
  ativo: boolean;
}

export interface CreateUserRequest {
  nome: string;
  email: string;
  senha: string;
  role: string;
}

export interface UpdateUserRequest {
  id: number;
  nome: string;
  email: string;
  role: string;
}
