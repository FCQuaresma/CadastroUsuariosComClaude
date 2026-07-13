import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { DropdownModule } from 'primeng/dropdown';
import { MessageService } from 'primeng/api';
import { UserService } from '../user.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonModule, InputTextModule, PasswordModule, DropdownModule],
  templateUrl: './user-form.component.html'
})
export class UserFormComponent implements OnInit {
  id: number | null = null;
  nome = '';
  email = '';
  senha = '';
  role = 'User';
  roles = [
    { label: 'Admin', value: 'Admin' },
    { label: 'User', value: 'User' }
  ];
  carregando = false;
  salvando = false;

  get modoEdicao(): boolean {
    return this.id !== null;
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id = Number(idParam);
      this.carregarUsuario(this.id);
    }
  }

  salvar(): void {
    this.salvando = true;

    if (this.modoEdicao) {
      this.userService
        .update(this.id!, { id: this.id!, nome: this.nome, email: this.email, role: this.role })
        .subscribe({ next: () => this.aoSalvarComSucesso(), error: erro => this.aoFalharAoSalvar(erro) });
    } else {
      this.userService
        .create({ nome: this.nome, email: this.email, senha: this.senha, role: this.role })
        .subscribe({ next: () => this.aoSalvarComSucesso(), error: erro => this.aoFalharAoSalvar(erro) });
    }
  }

  private aoSalvarComSucesso(): void {
    this.salvando = false;
    this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'Usuário salvo com sucesso.' });
    this.router.navigateByUrl('/users');
  }

  private aoFalharAoSalvar(erro: unknown): void {
    this.salvando = false;
    const mensagem = (erro as { error?: { message?: string } })?.error?.message ?? 'Não foi possível salvar o usuário.';
    this.messageService.add({ severity: 'error', summary: 'Erro', detail: mensagem });
  }

  private carregarUsuario(id: number): void {
    this.carregando = true;
    this.userService.getById(id).subscribe({
      next: usuario => {
        this.nome = usuario.nome;
        this.email = usuario.email;
        this.role = usuario.role;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.messageService.add({ severity: 'error', summary: 'Erro', detail: 'Não foi possível carregar o usuário.' });
      }
    });
  }
}
