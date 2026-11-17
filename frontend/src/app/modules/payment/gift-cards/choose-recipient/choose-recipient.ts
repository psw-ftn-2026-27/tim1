import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { RecipientDto } from '../../api/payment-api-types';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-choose-recipient',
  styleUrl: './choose-recipient.scss',
  templateUrl: './choose-recipient.html',
})
export class ChooseRecipient {
  protected readonly searchForm = form(signal({ text: '' }));

  protected readonly recipients = httpResource<PageResult<RecipientDto>>(
    () =>
      `/api/payment/gift-cards/recipients?text=${encodeURIComponent(this.searchForm.text().value())}&page=1&pageSize=20`,
  );
}
