import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';
import { UserService } from '../user.service';
import { UserViewModel } from '../user.model';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterLink, TableModule, ButtonModule, TagModule],
  templateUrl: './user-list.component.html'
})
export class UserListComponent {
  usuarios: UserViewModel[] = [];
  totalRegistros = 0;
  carregando = false;
  pageSize = 10;

  constructor(
    private userService: UserService,
    public authService: AuthService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  carregar(event: any): void {
    this.carregando = true;
    const pageSize = event.rows ?? this.pageSize;
    const page = Math.floor((event.first ?? 0) / pageSize) + 1;

    this.userService.getPaged(page, pageSize).subscribe({
      next: resultado => {
        this.usuarios = resultado.items;
        this.totalRegistros = resultado.totalCount;
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
        this.messageService.add({ severity: 'error', summary: 'Erro', detail: 'Não foi possível carregar os usuários.' });
      }
    });
  }

  confirmarInativacao(usuario: UserViewModel): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja inativar o usuário "${usuario.nome}"?`,
      header: 'Confirmar inativação',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.inativar(usuario)
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  private inativar(usuario: UserViewModel): void {
    this.userService.inactivate(usuario.id).subscribe({
      next: () => {
        this.usuarios = this.usuarios.filter(u => u.id !== usuario.id);
        this.totalRegistros--;
        this.messageService.add({ severity: 'success', summary: 'Sucesso', detail: 'Usuário inativado.' });
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Erro', detail: 'Não foi possível inativar o usuário.' });
      }
    });
  }
}
