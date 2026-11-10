import { canChangeStatus } from './task-status';

describe('canChangeStatus', () => {
  it('no deja pasar de Pending a Done', () => {
    expect(canChangeStatus('Pending', 'Done')).toBe(false);
  });

  it('permite las demas transiciones', () => {
    expect(canChangeStatus('Pending', 'InProgress')).toBe(true);
    expect(canChangeStatus('InProgress', 'Done')).toBe(true);
    expect(canChangeStatus('Done', 'Pending')).toBe(true);
  });
});
